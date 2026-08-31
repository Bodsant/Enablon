using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class AccessScopeFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AccessScopeFlowTests(WebApplicationFactory<Program> factory)
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
    public async Task Access_scope_member_scope_and_temporary_grant_flow()
    {
        await LoginAsync();
        var suffix = Guid.NewGuid().ToString("N")[..6];

        // 1. Get a real member id (for grant assignment).
        var membersResp = await _client.GetAsync("/api/v1/identities/members");
        membersResp.EnsureSuccessStatusCode();
        var members = await membersResp.Content.ReadFromJsonAsync<JsonElement>();
        var memberId = members.EnumerateArray().First().GetProperty("id").GetGuid();

        // 2. Create an access scope.
        var scopeResp = await _client.PostAsJsonAsync("/api/v1/identities/scopes", new
        {
            scopeType = "Site",
            companyId = (string?)null,
            businessUnitId = (string?)null,
            siteId = Guid.NewGuid(),          // free FK (site not modelled in identity)
            departmentId = (string?)null,
            locationId = (string?)null,
            contractorCompanyId = (string?)null,
            dataClassificationId = (string?)null,
        });
        scopeResp.EnsureSuccessStatusCode();
        var scopeDto = await scopeResp.Content.ReadFromJsonAsync<JsonElement>();
        var scopeId = scopeDto.GetProperty("id").GetGuid();
        Assert.Equal("Site", scopeDto.GetProperty("scopeType").GetString());

        // 3. List scopes contains it.
        var scopesResp = await _client.GetAsync("/api/v1/identities/scopes");
        scopesResp.EnsureSuccessStatusCode();
        var scopes = await scopesResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(scopes.EnumerateArray(), s => s.GetProperty("id").GetGuid() == scopeId);

        // 4. Grant the scope to the member.
        var grantScope = await _client.PostAsJsonAsync($"/api/v1/identities/members/{memberId}/scopes", new
        {
            accessScopeId = scopeId,
        });
        grantScope.EnsureSuccessStatusCode();
        var memberScopeDto = await grantScope.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(memberId, memberScopeDto.GetProperty("tenantMemberId").GetGuid());
        Assert.Equal(scopeId, memberScopeDto.GetProperty("accessScopeId").GetGuid());

        // 5. Member scopes list contains the grant.
        var memberScopes = await _client.GetAsync($"/api/v1/identities/members/{memberId}/scopes");
        memberScopes.EnsureSuccessStatusCode();
        var scopesArr = await memberScopes.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(scopesArr.EnumerateArray(), s => s.GetProperty("accessScopeId").GetGuid() == scopeId);

        // 6. Create a temporary access grant (approx approved by -> active member fallback).
        var grantResp = await _client.PostAsJsonAsync("/api/v1/identities/temporary-grants", new
        {
            tenantMemberId = memberId,
            accessScopeId = scopeId,
            roleId = (string?)null,
            approvedByMemberId = Guid.Empty,
            reason = "Integration test temporary access " + suffix,
            validFrom = DateTimeOffset.UtcNow.AddDays(-1).ToString("O"),
            validUntil = DateTimeOffset.UtcNow.AddDays(1).ToString("O"),
        });
        grantResp.EnsureSuccessStatusCode();
        var grantDto = await grantResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(memberId, grantDto.GetProperty("tenantMemberId").GetGuid());
        Assert.NotEqual(Guid.Empty, grantDto.GetProperty("approvedByMemberId").GetGuid());

        // 7. List temporary grants contains it.
        var grantsResp = await _client.GetAsync("/api/v1/identities/temporary-grants");
        grantsResp.EnsureSuccessStatusCode();
        var grants = await grantsResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(grants.EnumerateArray(), g => g.GetProperty("id").GetGuid() == grantDto.GetProperty("id").GetGuid());
    }
}