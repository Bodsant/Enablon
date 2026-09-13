using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

/// <summary>
/// STBL Sprint 39 — production-readiness probes verification.
///
/// Complements SecurityReadinessFlowTests (Sprint 36) by verifying the full
/// health-check surface (live + ready with real DB dependency) and that the
/// API fails closed on unknown/unauthenticated access.
/// </summary>
public sealed class ProductionReadinessFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductionReadinessFlowTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost"),
        });
    }

    private sealed record HealthPayload(string Status, string[]? Checks);

    [Fact]
    public async Task Liveness_endpoint_returns_healthy()
    {
        // Default ASP.NET health check writer: 200 + "Healthy" as text/plain.
        var resp = await _client.GetAsync("/health/live");
        resp.EnsureSuccessStatusCode();

        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", body);
    }

    [Fact]
    public async Task Readiness_endpoint_reports_healthy_after_db_check()
    {
        var resp = await _client.GetAsync("/health/ready");
        resp.EnsureSuccessStatusCode();
        Assert.Equal("application/json", resp.Content.Headers.ContentType?.MediaType);

        var body = await resp.Content.ReadFromJsonAsync<HealthPayload>();
        Assert.NotNull(body);
        Assert.Equal("Healthy", body!.Status);

        // The readiness probe includes the PostgreSQL dependency check.
        Assert.NotNull(body.Checks);
        Assert.Contains("postgres", body.Checks);
    }

    [Fact]
    public async Task Unknown_api_route_returns_404()
    {
        // Unmapped routes must 404 (fail closed) — never a 500.
        var resp = await _client.GetAsync("/api/v1/definitely-not-a-route");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Unauthorized_returns_401_not_500()
    {
        // Accessing a protected endpoint without credentials must fail closed
        // with a clean 401 — never a 500.
        var resp = await _client.GetAsync("/api/v1/audit-logs");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}