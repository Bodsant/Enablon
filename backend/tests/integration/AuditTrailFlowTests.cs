using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class AuditTrailFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuditTrailFlowTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost"),
        });
    }

    private async Task LoginAsync()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@ehsms.local",
            password = "EhsmsDev!123",
        });
        resp.EnsureSuccessStatusCode();
        var payload = await resp.Content.ReadFromJsonAsync<LoginPayload>();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", payload!.AccessToken);
    }

    public sealed record LoginPayload(string AccessToken, string TokenType);

    [Fact]
    public async Task Audit_log_is_queryable_and_read_only()
    {
        await LoginAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // The act of logging in / creating an asset writes audit entries via AuditLogWriter.
        // Create a small record to guarantee at least one audit entry exists this run.
        var asset = await _client.PostAsJsonAsync("/api/v1/assets", new
        {
            sourceSystem = "TEST",
            sourceId = $"SRC-{suffix}",
            assetCode = $"AST-{suffix}",
            assetName = $"Audit Probe {suffix}",
            assetType = "Probe",
            siteId = Guid.NewGuid(),
            locationId = (string?)null,
            isSafetyCritical = false,
            status = "Active",
        });
        asset.EnsureSuccessStatusCode();

        // 1. Query audit logs (tenant-scoped, read-only).
        var resp = await _client.GetAsync("/api/v1/audit-logs?limit=50");
        resp.EnsureSuccessStatusCode();
        var logs = await resp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(logs.GetArrayLength() >= 1, "Expected at least one audit log entry.");
        var first = logs.EnumerateArray().First();
        Assert.False(string.IsNullOrWhiteSpace(first.GetProperty("actionCode").GetString()));
        Assert.True(first.TryGetProperty("occurredAt", out _));
        Assert.True(first.GetProperty("id").GetString()!.Length > 0);

        // 2. Filter by a broad action filter (still returns entries).
        var filtered = await _client.GetAsync("/api/v1/audit-logs?limit=10&to=2099-12-31T23%3A59%3A59Z");
        filtered.EnsureSuccessStatusCode();
        var filteredLogs = await filtered.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(filteredLogs.GetArrayLength() >= 1);

        // 3. Append-only enforcement: there is NO create/update endpoint for audit logs.
        //    ASP.NET routing returns 405 Method Not Allowed for POST on a GET-only route.
        var post = await _client.PostAsJsonAsync("/api/v1/audit-logs", new
        {
            actionCode = "FORGED_AUDIT",
            afterJson = "{}",
            tenantId = Guid.NewGuid(),
        });
        Assert.Equal(System.Net.HttpStatusCode.MethodNotAllowed, post.StatusCode);
    }
}