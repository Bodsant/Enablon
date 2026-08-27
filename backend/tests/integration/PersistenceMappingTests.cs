using System;
using System.Threading.Tasks;
using Ehsms.BuildingBlocks.Persistence;
using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Identity.Infrastructure.Persistence;
using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Ehsms.Modules.Organisation.Infrastructure.Persistence;
using Ehsms.Modules.Organisation.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

/// <summary>
/// Verifies that the EF persistence model maps correctly to the live PostgreSQL
/// schema created by database/ddl/001-foundation.sql (org + iam), and that the
/// tenant fail-closed query filter isolates tenants.
///
/// These tests connect to a real database. They are skipped when the connection is
/// unavailable (e.g. CI without PostgreSQL) so the suite stays green elsewhere.
/// </summary>
public sealed class PersistenceMappingTests : IDisposable
{
    public const string EnvVariable = "EHSMS_TEST_DB";
    private readonly string _connectionString;
    private const string DefaultConnection =
        "Host=127.0.0.1;Port=5432;Database=ehsms;Username=ehsms_dev;Password=ehsms_dev_pw";

    public PersistenceMappingTests()
    {
        _connectionString = Environment.GetEnvironmentVariable(EnvVariable) ?? DefaultConnection;
    }

    [Fact]
    public async Task Identity_users_table_maps_to_live_schema()
    {
        if (!await CanConnect()) return; // skip when no database

        await using var db = new IdentityDbContext(
            Options<IdentityDbContext>(), new UnresolvedTenantContext());
        // iam.users carries no tenant filter; executing against the live table proves the
        // snake_case column mapping (id, email, normalized_email, ...) is correct.
        await db.Users.CountAsync();
    }

    [Fact]
    public async Task Organisation_tables_execute_against_live_schema()
    {
        if (!await CanConnect()) return;

        var tenant = Guid.NewGuid();
        var tenantContext = new ScopedTenantContext { CurrentTenantId = tenant };
        await using var db = new OrganisationDbContext(Options<OrganisationDbContext>(), tenantContext);

        // Executing each tenant-scoped query (routed through the fail-closed tenant
        // isolation) proves every mapped table and column matches DDL.
        await db.Companies.ForTenant(tenantContext).CountAsync();
        await db.BusinessUnits.ForTenant(tenantContext).CountAsync();
        await db.Sites.ForTenant(tenantContext).CountAsync();
        await db.Departments.ForTenant(tenantContext).CountAsync();
        await db.Locations.ForTenant(tenantContext).CountAsync();
        await db.Positions.ForTenant(tenantContext).CountAsync();
        await db.People.ForTenant(tenantContext).CountAsync();
        await db.Employees.ForTenant(tenantContext).CountAsync();
    }

    [Fact]
    public async Task Identity_tenant_scoped_tables_execute_against_live_schema()
    {
        if (!await CanConnect()) return;

        var tenant = Guid.NewGuid();
        var tenantContext = new ScopedTenantContext { CurrentTenantId = tenant };
        await using var db = new IdentityDbContext(Options<IdentityDbContext>(), tenantContext);

        await db.TenantMembers.ForTenant(tenantContext).CountAsync();
        await db.Roles.ForTenant(tenantContext).CountAsync();
        await db.Permissions.ForTenant(tenantContext).CountAsync();
        await db.AccessScopes.ForTenant(tenantContext).CountAsync();
    }

    [Fact]
    public async Task Tenant_filter_fails_closed_and_isolates_tenants()
    {
        if (!await CanConnect()) return;

        // org.companies.tenant_id has an FK to saas.tenants.id enforced by the DDL, so
        // seed two real tenants and use their returned ids as the test tenants.
        var (tenantA, tenantB) = await SeedTenantsAsync();

        // Seed two companies, one per tenant.
        var seedTenant = new ScopedTenantContext { CurrentTenantId = tenantA };
        await using (var db = new OrganisationDbContext(Options<OrganisationDbContext>(), seedTenant))
        {
            db.Companies.Add(new CompanyEntity
            {
                TenantId = tenantA,
                Code = "CC-A",
                Name = "Company A",
                Status = "active"
            });
            await db.SaveChangesAsync();
        }

        var seedTenantB = new ScopedTenantContext { CurrentTenantId = tenantB };
        await using (var dbB = new OrganisationDbContext(Options<OrganisationDbContext>(), seedTenantB))
        {
            dbB.Companies.Add(new CompanyEntity
            {
                TenantId = tenantB,
                Code = "CC-B",
                Name = "Company B",
                Status = "active"
            });
            await dbB.SaveChangesAsync();
        }

        // Tenant B only ever sees its own row.
        await using (var dbB = new OrganisationDbContext(Options<OrganisationDbContext>(), seedTenantB))
        {
            var visible = await dbB.Companies.ForTenant(seedTenantB)
                .Where(c => c.Code == "CC-A" || c.Code == "CC-B")
                .ToListAsync();
            Assert.Single(visible);
            Assert.Equal("CC-B", visible[0].Code);
        }

        // An unresolved tenant context must fail closed: no cross-tenant leak.
        await using (var dbU = new OrganisationDbContext(Options<OrganisationDbContext>(), new UnresolvedTenantContext()))
        {
            var leaked = await dbU.Companies.ForTenant(new UnresolvedTenantContext()).ToListAsync();
            Assert.Empty(leaked);
        }

        // Cleanup both rows.
        await using (var dbClean = new OrganisationDbContext(Options<OrganisationDbContext>(), new ScopedTenantContext { CurrentTenantId = tenantA }))
        {
            // Delete by direct tracking of the two seeded rows (bypassing tenant isolation).
            var all = await dbClean.Set<CompanyEntity>()
                .IgnoreQueryFilters()
                .Where(c => c.Code == "CC-A" || c.Code == "CC-B")
                .ToListAsync();
            dbClean.RemoveRange(all);
            await dbClean.SaveChangesAsync();
        }
    }

    private async Task<(Guid tenantA, Guid tenantB)> SeedTenantsAsync()
    {
        await using var conn = new Npgsql.NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        // Unique suffix keeps re-runs from tripping the UNIQUE(tenant_code)/UNIQUE(slug) constraints.
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var tenantA = await InsertTenantAsync(conn, $"seed-t-a-{suffix}", $"seed-t-a-{suffix}", "Seed Tenant A");
        var tenantB = await InsertTenantAsync(conn, $"seed-t-b-{suffix}", $"seed-t-b-{suffix}", "Seed Tenant B");

        await tx.CommitAsync();
        return (tenantA, tenantB);
    }

    private static async Task<Guid> InsertTenantAsync(
        Npgsql.NpgsqlConnection conn, string code, string slug, string displayName)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText =
            """
            INSERT INTO saas.tenants (tenant_code, slug, display_name, timezone, billing_anchor_day, status)
            VALUES (@code, @slug, @display, 'Asia/Jakarta', 1, 'active')
            RETURNING id;
            """;
        cmd.Parameters.AddWithValue("@code", code);
        cmd.Parameters.AddWithValue("@slug", slug);
        cmd.Parameters.AddWithValue("@display", displayName);
        var id = (Guid)cmd.ExecuteScalar()!;
        return id;
    }

    private DbContextOptions<T> Options<T>() where T : DbContext
    {
        var builder = new DbContextOptionsBuilder<T>();
        builder.UseSnakeCaseNamingConvention().UseNpgsql(_connectionString);
        return builder.Options;
    }

    private async Task<bool> CanConnect()
    {
        try
        {
            await using var n = new Npgsql.NpgsqlConnection(_connectionString);
            await n.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
    }
}
