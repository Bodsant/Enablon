using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="AccessScopeEntity"/> (<c>iam.access_scopes</c>).</summary>
public sealed class AccessScopeEntityConfiguration : IEntityTypeConfiguration<AccessScopeEntity>
{
    public const string TableName = "access_scopes";
    public const int ScopeTypeMaxLength = 30;

    public void Configure(EntityTypeBuilder<AccessScopeEntity> builder)
    {
        builder.ToTable(TableName, "iam");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ScopeType).IsRequired().HasMaxLength(ScopeTypeMaxLength);
        builder.Property(e => e.CompanyId);
        builder.Property(e => e.BusinessUnitId);
        builder.Property(e => e.SiteId);
        builder.Property(e => e.DepartmentId);
        builder.Property(e => e.LocationId);
        builder.Property(e => e.ContractorCompanyId);
        builder.Property(e => e.DataClassificationId);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_access_scopes_tenant_id");
        builder.HasIndex(e => new { e.TenantId, e.ScopeType }).HasDatabaseName("ix_access_scopes_tenant_id_scope_type");
        builder.HasIndex(e => e.CompanyId).HasDatabaseName("ix_access_scopes_company_id");
        builder.HasIndex(e => e.BusinessUnitId).HasDatabaseName("ix_access_scopes_business_unit_id");
        builder.HasIndex(e => e.SiteId).HasDatabaseName("ix_access_scopes_site_id");
        builder.HasIndex(e => e.DepartmentId).HasDatabaseName("ix_access_scopes_department_id");
        builder.HasIndex(e => e.LocationId).HasDatabaseName("ix_access_scopes_location_id");
    }
}