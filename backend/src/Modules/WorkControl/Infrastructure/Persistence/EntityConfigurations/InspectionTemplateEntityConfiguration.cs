using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="InspectionTemplateEntity"/> (<c>inspection.templates</c>).</summary>
public sealed class InspectionTemplateEntityConfiguration : IEntityTypeConfiguration<InspectionTemplateEntity>
{
    public const string TableName = "templates";

    public void Configure(EntityTypeBuilder<InspectionTemplateEntity> builder)
    {
        builder.ToTable(TableName, "inspection");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.InspectionType).HasMaxLength(60);
        builder.Property(e => e.OwnerMemberId).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_templates_tenant_id");
    }
}
