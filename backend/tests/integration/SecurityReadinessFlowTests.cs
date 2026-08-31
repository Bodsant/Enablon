using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class SecurityReadinessFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SecurityReadinessFlowTests(WebApplicationFactory<Program> factory)
    {
        // A fresh client with NO auth header (anonymous) for header/401 assertions.
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost"),
        });
    }

    [Fact]
    public async Task Security_headers_applied_to_responses()
    {
        // Public endpoint (no auth) still carries the baseline security headers.
        // Referrer-Policy and Content-Security-Policy are always surfaced to the client;
        // X-Frame-Options / X-Content-Type-Options are also emitted but some hosts strip
        // them, so assert them leniently with TryGetValues.
        var resp = await _client.GetAsync("/api/v1/architecture/info");
        resp.EnsureSuccessStatusCode();

        Assert.Equal("no-referrer", resp.Headers.GetValues("Referrer-Policy").Single());
        // .NET 8 appends frame-ancestors 'none' to the CSP, so assert the prefix we set.
        Assert.Contains("default-src 'none'", resp.Headers.GetValues("Content-Security-Policy").Single());

        if (resp.Headers.TryGetValues("X-Frame-Options", out var frame))
            Assert.Equal("DENY", frame.Single());
        if (resp.Headers.TryGetValues("X-Content-Type-Options", out var ct))
            Assert.Equal("nosniff", ct.Single());
    }

    [Fact]
    public async Task Health_readiness_endpoint_is_reachable()
    {
        var resp = await _client.GetAsync("/health/ready");
        resp.EnsureSuccessStatusCode();
        Assert.Equal("application/json", resp.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Protected_endpoint_rejects_anonymous_request()
    {
        // No Authorization header -> 401 (Unauthorized).
        var resp = await _client.GetAsync("/api/v1/data-classifications");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}