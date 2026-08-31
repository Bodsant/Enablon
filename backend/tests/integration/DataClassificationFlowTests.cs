using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class DataClassificationFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DataClassificationFlowTests(WebApplicationFactory<Program> factory)
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
    public async Task Data_classification_list_create_and_check_flow()
    {
        await LoginAsync();
        var suffix = Guid.NewGuid().ToString("N")[..6];
        var code = "SEC-" + suffix.ToUpperInvariant();

        // 1. Existing classifications are listed.
        var listResp = await _client.GetAsync("/api/v1/data-classifications");
        listResp.EnsureSuccessStatusCode();
        var list = await listResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(list.EnumerateArray().Any());

        // 2. Create a restricted classification.
        var createResp = await _client.PostAsJsonAsync("/api/v1/data-classifications", new
        {
            code,
            name = "Secret " + suffix,
            rank = 9,
            isRestricted = true,
        });
        createResp.EnsureSuccessStatusCode();
        var dto = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var id = dto.GetProperty("id").GetGuid();
        Assert.Equal(code, dto.GetProperty("code").GetString());
        Assert.True(dto.GetProperty("isRestricted").GetBoolean());

        // 3. Check returns restricted=true for it.
        var checkResp = await _client.PostAsJsonAsync("/api/v1/data-classifications/check", new { classificationId = id });
        checkResp.EnsureSuccessStatusCode();
        var check = await checkResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(check.GetProperty("restricted").GetBoolean());
        Assert.Equal(code, check.GetProperty("code").GetString());

        // 4. Fail-closed: unknown classification id is treated as restricted.
        var unknown = await _client.PostAsJsonAsync("/api/v1/data-classifications/check", new { classificationId = Guid.NewGuid() });
        unknown.EnsureSuccessStatusCode();
        var unknownCheck = await unknown.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(unknownCheck.GetProperty("restricted").GetBoolean());
    }
}