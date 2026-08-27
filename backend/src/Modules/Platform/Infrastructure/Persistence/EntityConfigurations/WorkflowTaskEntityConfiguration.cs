using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="WorkflowTaskEntity"/> (<c>platform.workflow_tasks</c>).</summary>
public sealed class WorkflowTaskEntityConfiguration : IEntityTypeConfiguration<WorkflowTaskEntity>
{
    public const string TableName = "workflow_tasks";
    public const int TaskTypeMaxLength = 40;
    public const int PriorityMaxLength = 20;
    public const int StatusMaxLength = 30;

    public void Configure(EntityTypeBuilder<WorkflowTaskEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.WorkflowInstanceId).IsRequired();
        builder.Property(e => e.TaskType).IsRequired().HasMaxLength(TaskTypeMaxLength);
        builder.Property(e => e.AssignedMemberId);
        builder.Property(e => e.AssignedRoleId);
        builder.Property(e => e.DueAt);
        builder.Property(e => e.Priority).HasMaxLength(PriorityMaxLength);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);
        builder.Property(e => e.CompletedAt);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_workflow_tasks_tenant_id");
        builder.HasIndex(e => e.WorkflowInstanceId).HasDatabaseName("ix_workflow_tasks_workflow_instance_id");
        builder.HasIndex(e => new { e.WorkflowInstanceId, e.Status }).HasDatabaseName("ix_workflow_tasks_instance_status");

        builder.HasOne(e => e.Instance)
            .WithMany(e => e.Tasks)
            .HasForeignKey(e => e.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}