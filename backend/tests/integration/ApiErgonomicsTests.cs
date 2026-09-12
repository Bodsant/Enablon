using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

/// <summary>
/// STBL Sprint 38/39 — production-readiness regression checks.
/// Verifies API ergonomics: endpoints should respond 200 with sane defaults
/// instead of 500 when optional query parameters are omitted.
/// </summary>
public sealed class ApiErgonomicsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public ApiErgonomicsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private sealed record LoginResponse(string AccessToken, string TokenType);

    private async Task<HttpClient> AuthorizedClientAsync()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Audit_logs_without_limit_param_returns_200()
    {
        using var client = await AuthorizedClientAsync();
        var resp = await client.GetAsync("/api/v1/audit-logs");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("[", body); // JSON array (possibly empty)
    }

    [Fact]
    public async Task Audit_logs_with_explicit_limit_returns_200()
    {
        using var client = await AuthorizedClientAsync();
        var resp = await client.GetAsync("/api/v1/audit-logs?limit=10");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
}