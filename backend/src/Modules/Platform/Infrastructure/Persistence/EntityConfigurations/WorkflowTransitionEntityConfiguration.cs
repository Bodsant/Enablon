using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="WorkflowTransitionEntity"/> (<c>platform.workflow_transitions</c>).</summary>
public sealed class WorkflowTransitionEntityConfiguration : IEntityTypeConfiguration<WorkflowTransitionEntity>
{
    public const string TableName = "workflow_transitions";
    public const int ActionCodeMaxLength = 50;

    public void Configure(EntityTypeBuilder<WorkflowTransitionEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.WorkflowVersionId).IsRequired();
        builder.Property(e => e.FromStateId).IsRequired();
        builder.Property(e => e.ToStateId).IsRequired();
        builder.Property(e => e.ActionCode).IsRequired().HasMaxLength(ActionCodeMaxLength);
        builder.Property(e => e.RequiredPermissionId);
        builder.Property(e => e.ValidationRuleJson).HasColumnType("jsonb");
        builder.Property(e => e.RequiresComment).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_workflow_transitions_tenant_id");
        builder.HasIndex(e => e.WorkflowVersionId).HasDatabaseName("ix_workflow_transitions_workflow_version_id");
        builder.HasIndex(e => e.FromStateId).HasDatabaseName("ix_workflow_transitions_from_state_id");
        builder.HasIndex(e => e.ToStateId).HasDatabaseName("ix_workflow_transitions_to_state_id");

        builder.HasOne(e => e.Version)
            .WithMany(e => e.Transitions)
            .HasForeignKey(e => e.WorkflowVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.FromState)
            .WithMany(e => e.FromTransitions)
            .HasForeignKey(e => e.FromStateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ToState)
            .WithMany(e => e.ToTransitions)
            .HasForeignKey(e => e.ToStateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}