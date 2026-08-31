using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class RetentionFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RetentionFlowTests(WebApplicationFactory<Program> factory)
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
    public async Task Retention_policy_create_list_and_purge_candidates_flow()
    {
        await LoginAsync();
        var recordType = "TEST-" + Guid.NewGuid().ToString("N")[..6];

        // 1. Create a retention policy with a short retention (read-only candidates API).
        var createResp = await _client.PostAsJsonAsync("/api/v1/retention-policies", new
        {
            recordType,
            classificationId = (string?)null,
            retentionDays = 30,
            archiveAfterDays = 1,
            recycleBinDays = 1,
            legalHoldSupported = true,
        });
        createResp.EnsureSuccessStatusCode();
        var dto = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var id = dto.GetProperty("id").GetGuid();
        Assert.Equal(recordType, dto.GetProperty("recordType").GetString());
        Assert.Equal(30, dto.GetProperty("retentionDays").GetInt32());

        // 2. List policies contains it.
        var listResp = await _client.GetAsync("/api/v1/retention-policies");
        listResp.EnsureSuccessStatusCode();
        var list = await listResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(list.EnumerateArray(), p => p.GetProperty("id").GetGuid() == id);

        // 3. Purge candidates endpoint returns an array (may be empty; never null and always 200).
        var candidatesResp = await _client.GetAsync("/api/v1/retention-policies/purge-candidates");
        candidatesResp.EnsureSuccessStatusCode();
        var candidates = await candidatesResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, candidates.ValueKind);
    }
}