using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="WorkflowInstanceEntity"/> (<c>platform.workflow_instances</c>).</summary>
public sealed class WorkflowInstanceEntityConfiguration : IEntityTypeConfiguration<WorkflowInstanceEntity>
{
    public const string TableName = "workflow_instances";

    public void Configure(EntityTypeBuilder<WorkflowInstanceEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.WorkflowVersionId).IsRequired();
        builder.Property(e => e.CurrentStateId).IsRequired();
        builder.Property(e => e.StartedAt).IsRequired();
        builder.Property(e => e.CompletedAt);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_workflow_instances_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_workflow_instances_record_id");
        builder.HasIndex(e => e.WorkflowVersionId).HasDatabaseName("ix_workflow_instances_workflow_version_id");
        builder.HasIndex(e => e.CurrentStateId).HasDatabaseName("ix_workflow_instances_current_state_id");

        builder.HasOne(e => e.Record)
            .WithMany(e => e.WorkflowInstances)
            .HasForeignKey(e => e.RecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Version)
            .WithMany(e => e.Instances)
            .HasForeignKey(e => e.WorkflowVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CurrentState)
            .WithMany(e => e.Instances)
            .HasForeignKey(e => e.CurrentStateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}