using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="WorkflowStateEntity"/> (<c>platform.workflow_states</c>).</summary>
public sealed class WorkflowStateEntityConfiguration : IEntityTypeConfiguration<WorkflowStateEntity>
{
    public const string TableName = "workflow_states";
    public const int StateCodeMaxLength = 50;
    public const int StateNameMaxLength = 100;

    public void Configure(EntityTypeBuilder<WorkflowStateEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.WorkflowVersionId).IsRequired();
        builder.Property(e => e.StateCode).IsRequired().HasMaxLength(StateCodeMaxLength);
        builder.Property(e => e.StateName).IsRequired().HasMaxLength(StateNameMaxLength);
        builder.Property(e => e.IsInitial).IsRequired();
        builder.Property(e => e.IsTerminal).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_workflow_states_tenant_id");
        builder.HasIndex(e => e.WorkflowVersionId).HasDatabaseName("ix_workflow_states_workflow_version_id");

        builder.HasOne(e => e.Version)
            .WithMany(e => e.States)
            .HasForeignKey(e => e.WorkflowVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}