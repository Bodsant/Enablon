using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="CapaActionEntity"/> (<c>capa.actions</c>).</summary>
public sealed class CapaActionEntityConfiguration : IEntityTypeConfiguration<CapaActionEntity>
{
    public const string TableName = "actions";

    public void Configure(EntityTypeBuilder<CapaActionEntity> builder)
    {
        builder.ToTable(TableName, "capa");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.ActionType).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.OwnerMemberId).IsRequired();
        builder.Property(e => e.Priority).IsRequired().HasMaxLength(20);
        builder.Property(e => e.DueDate).IsRequired();
        builder.Property(e => e.ProgressPercentage).IsRequired();
        builder.Property(e => e.VerificationRequired).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_actions_tenant_id");
    }
}
