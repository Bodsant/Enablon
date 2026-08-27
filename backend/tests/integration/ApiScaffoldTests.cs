using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class ApiScaffoldTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Csp = "default-src 'none'; frame-ancestors 'none'";
    private readonly HttpClient _client;
    public ApiScaffoldTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Architecture_info_reflects_wired_modular_monolith()
    {
        var response = await _client.GetAsync("/api/v1/architecture/info");
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ArchitectureInfo>();
        Assert.Equal("ENABLON EHSMS", payload?.Name);
        Assert.Equal("modular-monolith", payload?.Capability);
        Assert.True(payload is { BusinessFeaturesImplemented: true, Persistence.Database: "postgresql" });
        Assert.Equal("not-configured", payload?.Authentication);
    }

    [Fact]
    public async Task Liveness_is_independent_from_dependencies() =>
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/health/live")).StatusCode);

    [Fact]
    public async Task Readiness_selects_the_named_process_self_check()
    {
        var response = await _client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("process-readiness", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Supplied_correlation_and_exact_security_headers_are_returned()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/architecture/info");
        request.Headers.Add("X-Correlation-ID", "test-correlation");
        var response = await _client.SendAsync(request);
        Assert.Equal("test-correlation", Header(response, "X-Correlation-ID"));
        Assert.Equal("nosniff", Header(response, "X-Content-Type-Options"));
        Assert.Equal("no-referrer", Header(response, "Referrer-Policy"));
        Assert.Equal(Csp, Header(response, "Content-Security-Policy"));
    }

    [Fact]
    public async Task Correlation_id_is_generated_when_absent()
    {
        var response = await _client.GetAsync("/api/v1/architecture/info");
        var correlationId = Header(response, "X-Correlation-ID");
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
        Assert.Equal("nosniff", Header(response, "X-Content-Type-Options"));
        Assert.Equal("no-referrer", Header(response, "Referrer-Policy"));
        Assert.Equal(Csp, Header(response, "Content-Security-Policy"));
    }

    private static string Header(HttpResponseMessage response, string name) =>
        response.Headers.GetValues(name).Single();

    private sealed record ArchitectureInfo(
        string Name,
        string Capability,
        bool BusinessFeaturesImplemented,
        string Authentication,
        PersistenceInfo? Persistence);

    private sealed record PersistenceInfo(string Database, string[] Modules);
}
