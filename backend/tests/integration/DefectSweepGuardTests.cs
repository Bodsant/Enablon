using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

/// <summary>
/// STBL Sprint 40 — defect sweep guard rails.
///
/// Verifies the parameter-clamping behaviour added for API ergonomics stays
/// correct: out-of-range pagination values are clamped instead of crashing,
/// and boundary values behave sanely. Guards against regression of the
/// /api/v1/audit-logs fix (Sprint 38) and similar list endpoints.
/// </summary>
public sealed class DefectSweepGuardTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public DefectSweepGuardTests(WebApplicationFactory<Program> factory) => _factory = factory;

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

    [Theory]
    [InlineData("limit=0")]
    [InlineData("limit=-5")]
    [InlineData("limit=1")]
    [InlineData("limit=500")]
    [InlineData("limit=999999")]
    [InlineData("")] // no param at all
    public async Task Audit_logs_clamps_limit_without_crashing(string query)
    {
        using var client = await AuthorizedClientAsync();
        var path = string.IsNullOrEmpty(query) ? "/api/v1/audit-logs" : $"/api/v1/audit-logs?{query}";

        var resp = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var body = await resp.Content.ReadFromJsonAsync<object[]>();
        Assert.NotNull(body);
    }

    [Fact]
    public async Task Audit_logs_garbage_limit_returns_400_or_200_never_500()
    {
        using var client = await AuthorizedClientAsync();
        // Non-numeric limit: binding fails cleanly — either 400 (bad request)
        // or clamped 200, but NEVER a 500.
        var resp = await client.GetAsync("/api/v1/audit-logs?limit=abc");
        Assert.True(resp.StatusCode is HttpStatusCode.OK or HttpStatusCode.BadRequest,
            $"unexpected status {resp.StatusCode}");
    }
}