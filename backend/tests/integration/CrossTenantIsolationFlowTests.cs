using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ehsms.Api.Authentication;
using Ehsms.Modules.Identity.Infrastructure.Authentication;
using Ehsms.Modules.Identity.Infrastructure.Persistence;
using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

/// <summary>
/// STBL Sprint 37 — cross-tenant negative authorization (OWASP-style).
/// Verifies that a member of tenant B cannot observe, list, or directly fetch
/// data owned by tenant A, and fails closed rather than leaking.
/// </summary>
public sealed class CrossTenantIsolationFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public CrossTenantIsolationFlowTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private sealed record LoginResponse(string AccessToken, string TokenType);
    private sealed record RecordPayload(Guid Id);

    [Fact]
    public async Task TenantB_cannot_read_or_list_TenantA_records()
    {
        using var client = _factory.CreateClient();

        // ---- Tenant A: login as the seeded dev admin and confirm its tenant. ----
        var aLogin = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        Assert.Equal(HttpStatusCode.OK, aLogin.StatusCode);
        var aToken = (await aLogin.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", aToken);

        // The admin is always provisioned into the tenant with the lowest tenant id
        // (Program.cs dev bootstrap: saas.Tenants.OrderBy(t => t.Id).First()).
        Guid tenantAId;
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var saas = scope.ServiceProvider.GetRequiredService<Ehsms.Modules.Saas.Infrastructure.Persistence.SaasDbContext>();
            tenantAId = (await saas.Tenants.OrderBy(t => t.Id).FirstOrDefaultAsync())!.Id;
        }

        // Tenant A creates a record the other tenant must never see.
        var createRec = await client.PostAsJsonAsync("/api/v1/platform/records",
            new { moduleCode = "HSE", recordType = "INCIDENT", title = "Tenant A secret record", dataClassificationId = Guid.NewGuid() });
        Assert.Equal(HttpStatusCode.Created, createRec.StatusCode);
        var recordA = (await createRec.Content.ReadFromJsonAsync<RecordPayload>())!.Id;

        // ---- Tenant B: create an isolated tenant + a login-able member. ----
        Guid tenantB;
        var suffix = Guid.NewGuid().ToString("N")[..8];
        using (var conn = new NpgsqlConnection(ConnectionString()))
        {
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText =
                """
                INSERT INTO saas.tenants (tenant_code, slug, display_name, timezone, billing_anchor_day, status)
                VALUES (@code, @slug, @display, 'Asia/Jakarta', 1, 'active')
                RETURNING id;
                """;
            cmd.Parameters.AddWithValue("@code", $"stbl-b-{suffix}");
            cmd.Parameters.AddWithValue("@slug", $"stbl-b-{suffix}");
            cmd.Parameters.AddWithValue("@display", "Stabilization Tenant B");
            tenantB = (Guid)cmd.ExecuteScalar()!;
        }

        string bPassword = "TenantB!Pass123";
        Guid userBId = Guid.NewGuid();
        Guid memberBId = Guid.NewGuid();
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var idb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            idb.Users.Add(new UserEntity
            {
                Id = userBId,
                Email = $"b-{suffix}@ehsms.dev",
                NormalizedEmail = $"B-{suffix}@EHSMS.DEV",
                PasswordHash = hasher.Hash(bPassword),
                IdentityProvider = "local",
                Status = "Active",
            });
            idb.TenantMembers.Add(new TenantMemberEntity
            {
                Id = memberBId,
                TenantId = tenantB,
                UserId = userBId,
                DisplayName = "Tenant B Operator",
                Status = "Active",
                ActivatedAt = DateTimeOffset.UtcNow,
            });
            await idb.SaveChangesAsync();
        }

        // Issue an access token for the tenant B member directly via the same
        // JwtTokenService the login endpoint uses. This keeps the test focused on
        // tenant isolation (the concern under test) rather than the password-auth
        // path (already covered by AuthFlowTests).
        using var bClient = _factory.CreateClient();
        string bToken;
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var jwt = scope.ServiceProvider.GetRequiredService<JwtTokenService>();
            (bToken, _) = jwt.CreateAccessToken(userBId, $"b-{suffix}@ehsms.dev", tenantB);
        }
        bClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bToken);

        // Tenant B confirms it resolves to its own tenant (not tenant A).
        var me = await bClient.GetFromJsonAsync<MePayload>("/api/v1/auth/me");
        Assert.NotNull(me);
        Assert.Equal(tenantB, me.TenantId);
        Assert.NotEqual(tenantAId, me.TenantId);

        // ---- Negative assertions: tenant B must fail closed on tenant A data. ----
        // 1. Direct fetch of tenant A's record by id must NOT return it.
        var fetched = await bClient.GetAsync($"/api/v1/platform/records/{recordA}");
        Assert.NotEqual(HttpStatusCode.OK, fetched.StatusCode);

        // 2. Listing records as tenant B must not include tenant A's record.
        var listB = (await bClient.GetFromJsonAsync<RecordRow[]>("/api/v1/platform/records"))!;
        Assert.DoesNotContain(listB, r => r.Id == recordA);

        // 3. Listing records as tenant A still sees its own record (sanity).
        var listA = (await client.GetFromJsonAsync<RecordRow[]>("/api/v1/platform/records"))!;
        Assert.Contains(listA, r => r.Id == recordA);
    }

    private sealed record MePayload(Guid? TenantId, string Email);
    private sealed record RecordRow(Guid Id, string? RecordType, string? Title);

    private static string ConnectionString() =>
        Environment.GetEnvironmentVariable("EHSMS_TEST_DB")
        ?? Environment.GetEnvironmentVariable("ConnectionStrings__EhSms")
        ?? "Host=127.0.0.1;Port=5432;Database=ehsms;Username=ehsms_dev;Password=ehsms_dev_pw";
}
