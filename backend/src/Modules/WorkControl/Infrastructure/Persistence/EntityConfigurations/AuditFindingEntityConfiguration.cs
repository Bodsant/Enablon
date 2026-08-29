using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="AuditFindingEntity"/> (<c>audit.findings</c>).</summary>
public sealed class AuditFindingEntityConfiguration : IEntityTypeConfiguration<AuditFindingEntity>
{
    public const string TableName = "findings";

    public void Configure(EntityTypeBuilder<AuditFindingEntity> builder)
    {
        builder.ToTable(TableName, "audit");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.AuditId).IsRequired();
        builder.Property(e => e.AuditResponseId);
        builder.Property(e => e.Classification).HasMaxLength(40).IsRequired();
        builder.Property(e => e.RequirementReference).HasMaxLength(200);
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.Recommendation);
        builder.Property(e => e.OwnerMemberId);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_findings_tenant_id");
    }
}
