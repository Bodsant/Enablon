using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class IntegrationFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IntegrationFlowTests(WebApplicationFactory<Program> factory)
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

    private static Guid GuidFrom(object value) => Guid.Parse(value!.ToString()!);

    [Fact]
    public async Task Interface_mapping_run_message_reconciliation_flow()
    {
        await LoginAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // 1. Integration interface (owner nullable -> null).
        var iface = await _client.PostAsJsonAsync("/api/v1/integration/interfaces", new
        {
            code = $"HRIS-{suffix}",
            name = $"HRIS Employee Sync {suffix}",
            sourceSystem = "HRIS",
            targetSystem = "EHSMS",
            integrationMethod = "API",
            authenticationType = "OAuth2",
            ownerMemberId = "00000000-0000-0000-0000-000000000000",
            status = "Active",
        });
        iface.EnsureSuccessStatusCode();
        var ifaceDto = await iface.Content.ReadFromJsonAsync<JsonElement>();
        var ifaceId = GuidFrom(ifaceDto.GetProperty("id"));
        Assert.Equal(("HRIS-" + suffix).ToUpperInvariant(), ifaceDto.GetProperty("code").GetString());

        // 2. Data mapping (interface FK valid).
        var mapping = await _client.PostAsJsonAsync("/api/v1/integration/data-mappings", new
        {
            interfaceId = ifaceId,
            versionNumber = 1,
            sourceSchemaJson = "{\"employee\":\"id\"}",
            targetSchemaJson = "{\"person\":\"external_id\"}",
            mappingRulesJson = "{\"map\":[\"employee.id->person.external_id\"]}",
            effectiveFrom = "2026-01-01T00:00:00Z",
        });
        mapping.EnsureSuccessStatusCode();
        var mapDto = await mapping.Content.ReadFromJsonAsync<JsonElement>();
        var mapId = GuidFrom(mapDto.GetProperty("id"));
        Assert.Equal(1, mapDto.GetProperty("versionNumber").GetInt32());

        // 3. Run (interface + optional mapping).
        var run = await _client.PostAsJsonAsync("/api/v1/integration/runs", new
        {
            interfaceId = ifaceId,
            mappingId = mapId,
            correlationId = $"run-{suffix}",
            status = "Completed",
            receivedCount = 10,
            successCount = 8,
            errorCount = 2,
        });
        run.EnsureSuccessStatusCode();
        var runDto = await run.Content.ReadFromJsonAsync<JsonElement>();
        var runId = GuidFrom(runDto.GetProperty("id"));
        Assert.Equal(10, runDto.GetProperty("receivedCount").GetInt64());
        Assert.Equal(8, runDto.GetProperty("successCount").GetInt64());

        // 4. Message (run FK valid).
        var msg = await _client.PostAsJsonAsync("/api/v1/integration/messages", new
        {
            integrationRunId = runId,
            externalKey = $"emp-{suffix}-001",
            payloadHash = "abc123",
            processingStatus = "Processed",
            errorCode = (string?)null,
            errorMessage = (string?)null,
            retryCount = 0,
        });
        msg.EnsureSuccessStatusCode();
        var msgDto = await msg.Content.ReadFromJsonAsync<JsonElement>();
        var msgId = GuidFrom(msgDto.GetProperty("id"));
        Assert.Equal("Processed", msgDto.GetProperty("processingStatus").GetString());

        // 5. Reconciliation (run FK valid; approver nullable -> null).
        var recon = await _client.PostAsJsonAsync("/api/v1/integration/reconciliations", new
        {
            integrationRunId = runId,
            sourceCount = 10,
            targetCount = 8,
            matchedCount = 8,
            unmatchedCount = 2,
            status = "Open",
            approvedByMemberId = "00000000-0000-0000-0000-000000000000",
        });
        recon.EnsureSuccessStatusCode();
        var reconDto = await recon.Content.ReadFromJsonAsync<JsonElement>();
        var reconId = GuidFrom(reconDto.GetProperty("id"));

        // 6. Verify round-trips.
        var interfaces = await (await _client.GetAsync("/api/v1/integration/interfaces")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(interfaces.EnumerateArray(), i => i.GetProperty("id").GetString() == ifaceId.ToString());

        var mappings = await (await _client.GetAsync($"/api/v1/integration/data-mappings?interfaceId={ifaceId}")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(mappings.EnumerateArray(), m => m.GetProperty("id").GetString() == mapId.ToString());

        var runs = await (await _client.GetAsync($"/api/v1/integration/runs?interfaceId={ifaceId}")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(runs.EnumerateArray(), r => r.GetProperty("id").GetString() == runId.ToString());

        var messages = await (await _client.GetAsync($"/api/v1/integration/messages?integrationRunId={runId}")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(messages.EnumerateArray(), m => m.GetProperty("id").GetString() == msgId.ToString());

        var reconciliations = await (await _client.GetAsync($"/api/v1/integration/reconciliations?integrationRunId={runId}")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(reconciliations.EnumerateArray(), r => r.GetProperty("id").GetString() == reconId.ToString());
    }
}