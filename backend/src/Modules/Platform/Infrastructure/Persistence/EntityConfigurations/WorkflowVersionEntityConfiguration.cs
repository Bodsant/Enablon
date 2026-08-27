using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="WorkflowVersionEntity"/> (<c>platform.workflow_versions</c>).</summary>
public sealed class WorkflowVersionEntityConfiguration : IEntityTypeConfiguration<WorkflowVersionEntity>
{
    public const string TableName = "workflow_versions";
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<WorkflowVersionEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.WorkflowDefinitionId).IsRequired();
        builder.Property(e => e.VersionNumber).IsRequired();
        builder.Property(e => e.EffectiveFrom).IsRequired();
        builder.Property(e => e.EffectiveTo);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_workflow_versions_tenant_id");
        builder.HasIndex(e => e.WorkflowDefinitionId).HasDatabaseName("ix_workflow_versions_workflow_definition_id");
        builder.HasIndex(e => new { e.WorkflowDefinitionId, e.VersionNumber }).HasDatabaseName("ix_workflow_versions_definition_version");

        builder.HasOne(e => e.Definition)
            .WithMany(e => e.Versions)
            .HasForeignKey(e => e.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}