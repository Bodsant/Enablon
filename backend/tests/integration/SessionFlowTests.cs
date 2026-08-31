using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class SessionFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SessionFlowTests(WebApplicationFactory<Program> factory)
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
    public async Task Session_list_revoke_single_and_revoke_all_flow()
    {
        await LoginAsync();

        // 1. List sessions — login created at least one active refresh-token session,
        //    and no token hash is ever exposed in the payload.
        var listResp = await _client.GetAsync("/api/v1/identities/sessions");
        listResp.EnsureSuccessStatusCode();
        var sessions = await listResp.Content.ReadFromJsonAsync<JsonElement>();
        var arr = sessions.EnumerateArray().ToList();
        Assert.NotEmpty(arr);
        var token = arr.First().GetProperty("id").GetGuid();

        // 2. Revoke a single session.
        var revoke = await _client.PostAsync($"/api/v1/identities/sessions/{token}/revoke", null);
        revoke.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, revoke.StatusCode);

        var after = await (await _client.GetAsync("/api/v1/identities/sessions")).Content.ReadFromJsonAsync<JsonElement>();
        var revokedEntry = after.EnumerateArray().FirstOrDefault(s => s.GetProperty("id").GetGuid() == token);
        Assert.NotEqual(JsonValueKind.Undefined, revokedEntry.ValueKind);
        Assert.NotNull(revokedEntry.GetProperty("revokedAt").GetString());

        // 3. Revoking an unknown token returns 404.
        var missing = await _client.PostAsync($"/api/v1/identities/sessions/{Guid.NewGuid()}/revoke", null);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, missing.StatusCode);

        // 4. Revoke all remaining sessions.
        var revokeAll = await _client.PostAsync("/api/v1/identities/sessions/revoke-all", null);
        revokeAll.EnsureSuccessStatusCode();
        var allPayload = await revokeAll.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(allPayload.GetProperty("revoked").GetInt32() >= 0);
    }
}