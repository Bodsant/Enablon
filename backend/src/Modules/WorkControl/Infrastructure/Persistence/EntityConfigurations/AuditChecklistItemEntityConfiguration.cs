using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="AuditChecklistItemEntity"/> (<c>audit.checklist_items</c>).</summary>
public sealed class AuditChecklistItemEntityConfiguration : IEntityTypeConfiguration<AuditChecklistItemEntity>
{
    public const string TableName = "checklist_items";

    public void Configure(EntityTypeBuilder<AuditChecklistItemEntity> builder)
    {
        builder.ToTable(TableName, "audit");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ChecklistTemplateId).IsRequired();
        builder.Property(e => e.SequenceNumber).IsRequired();
        builder.Property(e => e.RequirementReference).HasMaxLength(200);
        builder.Property(e => e.Prompt).IsRequired();
        builder.Property(e => e.ClassificationRuleJson).HasColumnType("jsonb");

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_checklist_items_tenant_id");
    }
}
