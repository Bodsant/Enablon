using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="AuditChecklistTemplateEntity"/> (<c>audit.checklist_templates</c>).</summary>
public sealed class AuditChecklistTemplateEntityConfiguration : IEntityTypeConfiguration<AuditChecklistTemplateEntity>
{
    public const string TableName = "checklist_templates";

    public void Configure(EntityTypeBuilder<AuditChecklistTemplateEntity> builder)
    {
        builder.ToTable(TableName, "audit");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.StandardReference).HasMaxLength(200);
        builder.Property(e => e.VersionNumber).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_checklist_templates_tenant_id");
    }
}
