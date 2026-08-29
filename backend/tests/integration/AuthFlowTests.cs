using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class AuthFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public AuthFlowTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Login_with_valid_credentials_returns_tokens()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = "admin@ehsms.local", password = "EhsmsDev!123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrWhiteSpace(payload?.AccessToken));
        Assert.Equal("Bearer", payload?.TokenType);
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = "admin@ehsms.local", password = "wrong" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Protected_endpoint_rejects_anonymous_requests()
    {
        var response = await _client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public sealed record LoginResponse(string AccessToken, string TokenType);
}