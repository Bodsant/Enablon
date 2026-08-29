using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="AuditEntity"/> (<c>audit.audits</c>).</summary>
public sealed class AuditEntityConfiguration : IEntityTypeConfiguration<AuditEntity>
{
    public const string TableName = "audits";

    public void Configure(EntityTypeBuilder<AuditEntity> builder)
    {
        builder.ToTable(TableName, "audit");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.AuditProgramId);
        builder.Property(e => e.ChecklistTemplateId);
        builder.Property(e => e.AuditType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.ScopeText).IsRequired();
        builder.Property(e => e.CriteriaText);
        builder.Property(e => e.LeadAuditorMemberId).IsRequired();
        builder.Property(e => e.ScheduledStart);
        builder.Property(e => e.ScheduledEnd);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_audits_tenant_id");
    }
}
