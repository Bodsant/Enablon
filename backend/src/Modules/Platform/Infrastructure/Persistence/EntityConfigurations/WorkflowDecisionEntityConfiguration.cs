using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="WorkflowDecisionEntity"/> (<c>platform.workflow_decisions</c>).</summary>
public sealed class WorkflowDecisionEntityConfiguration : IEntityTypeConfiguration<WorkflowDecisionEntity>
{
    public const string TableName = "workflow_decisions";
    public const int DecisionMaxLength = 30;

    public void Configure(EntityTypeBuilder<WorkflowDecisionEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.WorkflowTaskId).IsRequired();
        builder.Property(e => e.TransitionId);
        builder.Property(e => e.Decision).IsRequired().HasMaxLength(DecisionMaxLength);
        builder.Property(e => e.Comment);
        builder.Property(e => e.DecidedByMemberId).IsRequired();
        builder.Property(e => e.DecidedAt).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_workflow_decisions_tenant_id");
        builder.HasIndex(e => e.WorkflowTaskId).HasDatabaseName("ix_workflow_decisions_workflow_task_id");
        builder.HasIndex(e => e.TransitionId).HasDatabaseName("ix_workflow_decisions_transition_id");

        builder.HasOne(e => e.Task)
            .WithMany(e => e.Decisions)
            .HasForeignKey(e => e.WorkflowTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Transition)
            .WithMany(e => e.Decisions)
            .HasForeignKey(e => e.TransitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}