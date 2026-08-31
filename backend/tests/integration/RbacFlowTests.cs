using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class RbacFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RbacFlowTests(WebApplicationFactory<Program> factory)
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
    public async Task Rbac_role_permission_and_member_assignment_flow()
    {
        await LoginAsync();
        var suffix = Guid.NewGuid().ToString("N")[..6];
        var roleCode = "AUD-" + suffix.ToUpperInvariant();

        // 1. Create a role.
        var created = await _client.PostAsJsonAsync("/api/v1/identities/roles", new
        {
            code = roleCode,
            name = "Audit Role " + suffix,
            scopeType = "Company",
            isSystem = false,
        });
        created.EnsureSuccessStatusCode();
        var roleDto = await created.Content.ReadFromJsonAsync<JsonElement>();
        var roleId = roleDto.GetProperty("id").GetGuid();
        Assert.Equal(roleCode, roleDto.GetProperty("code").GetString());

        // 2. List roles contains it.
        var rolesResp = await _client.GetAsync("/api/v1/identities/roles");
        rolesResp.EnsureSuccessStatusCode();
        var roles = await rolesResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(roles.EnumerateArray(), r => r.GetProperty("id").GetGuid() == roleId);

        // 3. Create a permission in this tenant, then attach it to the role.
        var permCode = "RN-" + suffix.ToUpperInvariant();
        var createPerm = await _client.PostAsJsonAsync("/api/v1/identities/permissions", new
        {
            code = permCode,
            module = "audit",
            action = "read",
            description = "Integration test permission",
        });
        createPerm.EnsureSuccessStatusCode();
        var permDto = await createPerm.Content.ReadFromJsonAsync<JsonElement>();
        var permissionId = permDto.GetProperty("id").GetGuid();
        Assert.Equal(permCode, permDto.GetProperty("code").GetString());

        var attach = await _client.PostAsJsonAsync($"/api/v1/identities/roles/{roleId}/permissions", new
        {
            permissionId,
        });
        attach.EnsureSuccessStatusCode();
        var linkDto = await attach.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(roleId, linkDto.GetProperty("roleId").GetGuid());
        Assert.Equal(permissionId, linkDto.GetProperty("permissionId").GetGuid());

        // 4. Members exist; assign the new role to the first member (idempotent-safe in test).
        var membersResp = await _client.GetAsync("/api/v1/identities/members");
        membersResp.EnsureSuccessStatusCode();
        var members = await membersResp.Content.ReadFromJsonAsync<JsonElement>();
        var memberId = members.EnumerateArray().First().GetProperty("id").GetGuid();

        var assign = await _client.PostAsJsonAsync($"/api/v1/identities/members/{memberId}/roles", new
        {
            roleId,
        });
        assign.EnsureSuccessStatusCode();
        var memberRoleDto = await assign.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(memberId, memberRoleDto.GetProperty("tenantMemberId").GetGuid());
        Assert.Equal(roleId, memberRoleDto.GetProperty("roleId").GetGuid());
    }
}