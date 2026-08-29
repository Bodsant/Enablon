using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class PlatformRecordFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    public PlatformRecordFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_token_carries_tenant_claim_after_dev_seed()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var payload = await login.Content.ReadFromJsonAsync<LoginPayload>();
        Assert.False(string.IsNullOrEmpty(payload?.AccessToken));

        // Decode the JWT payload (middle segment) without external deps.
        var parts = payload.AccessToken.Split('.');
        var json = Base64UrlDecode(parts[1]);
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("tenant", out var tenantClaim), "JWT should carry a tenant claim after dev seed assigns membership");
        Assert.False(string.IsNullOrEmpty(tenantClaim.GetString()));
    }

    [Fact]
    public async Task Create_record_succeeds_with_tenant_scoped_token()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        var payload = await login.Content.ReadFromJsonAsync<LoginPayload>();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", payload!.AccessToken);

        var create = await _client.PostAsJsonAsync("/api/v1/platform/records", new
        {
            moduleCode = "HSE",
            recordType = "Incident",
            title = "Integration record",
            dataClassificationId = "00000000-0000-0000-0000-000000000001",
        });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<CreateRecordPayload>();
        Assert.False(string.IsNullOrEmpty(created?.RecordNumber));
        Assert.StartsWith("HSE-", created.RecordNumber);
    }

    private static string Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(s));
    }

    public sealed record LoginPayload(string AccessToken, string TokenType);
    public sealed record CreateRecordPayload(Guid Id, string RecordNumber, string Status);
}