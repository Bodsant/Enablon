using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="InspectionFindingEntity"/> (<c>inspection.findings</c>).</summary>
public sealed class InspectionFindingEntityConfiguration : IEntityTypeConfiguration<InspectionFindingEntity>
{
    public const string TableName = "findings";

    public void Configure(EntityTypeBuilder<InspectionFindingEntity> builder)
    {
        builder.ToTable(TableName, "inspection");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.InspectionId).IsRequired();
        builder.Property(e => e.ResponseId);
        builder.Property(e => e.Classification).HasMaxLength(40);
        builder.Property(e => e.SeverityId);
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.OwnerMemberId);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_findings_tenant_id");
    }
}
