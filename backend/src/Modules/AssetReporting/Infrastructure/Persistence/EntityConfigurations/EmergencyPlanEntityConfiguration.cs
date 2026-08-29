using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="EmergencyPlanEntity"/> (<c>emergency.plans</c>).</summary>
public sealed class EmergencyPlanEntityConfiguration : IEntityTypeConfiguration<EmergencyPlanEntity>
{
    public const string TableName = "plans";
    public const int CodeMaxLength = 50;
    public const int NameMaxLength = 200;
    public const int StatusMaxLength = 30;

    public void Configure(EntityTypeBuilder<EmergencyPlanEntity> builder)
    {
        builder.ToTable(TableName, "emergency");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(CodeMaxLength);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(NameMaxLength);
        builder.Property(e => e.SiteId).IsRequired();
        builder.Property(e => e.OwnerMemberId).IsRequired();
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_plans_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_plans_record_id");
        builder.HasIndex(e => e.Code).HasDatabaseName("ix_plans_code");
        builder.HasIndex(e => e.SiteId).HasDatabaseName("ix_plans_site_id");
        builder.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("ix_plans_tenant_id_status");
    }
}