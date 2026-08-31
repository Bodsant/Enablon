using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class AccessReviewFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AccessReviewFlowTests(WebApplicationFactory<Program> factory)
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
    public async Task Access_review_create_and_list_with_resolved_reviewer()
    {
        await LoginAsync();
        var suffix = Guid.NewGuid().ToString("N")[..6];

        // ReviewerMemberId is Guid.Empty -> service falls back to the resolved active member.
        var resp = await _client.PostAsJsonAsync("/api/v1/access-reviews", new
        {
            reviewPeriodStart = "2026-09-01",
            reviewPeriodEnd = "2026-10-01",
            reviewerMemberId = Guid.Empty,
            status = "OPEN",
        });
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync<JsonElement>();
        var reviewerId = dto.GetProperty("reviewerMemberId").GetGuid();
        Assert.NotEqual(Guid.Empty, reviewerId);   // resolved, not empty
        Assert.Equal("OPEN", dto.GetProperty("status").GetString());
        Assert.True(dto.TryGetProperty("completedAt", out _));

        // List returns the created review.
        var list = await _client.GetAsync("/api/v1/access-reviews");
        list.EnsureSuccessStatusCode();
        var arr = await list.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(arr.GetArrayLength() >= 1);

        // An invalid reviewer is rejected (FK fk_review_owner → iam.tenant_members).
        var bad = await _client.PostAsJsonAsync("/api/v1/access-reviews", new
        {
            reviewPeriodStart = "2026-09-01",
            reviewPeriodEnd = "2026-10-01",
            reviewerMemberId = Guid.NewGuid(),
            status = "OPEN",
        });
        Assert.False(bad.IsSuccessStatusCode);
    }
}