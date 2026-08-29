using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="WorkExecutionEntity"/> (<c>cow.work_executions</c>).</summary>
public sealed class WorkExecutionEntityConfiguration : IEntityTypeConfiguration<WorkExecutionEntity>
{
    public const string TableName = "work_executions";

    public void Configure(EntityTypeBuilder<WorkExecutionEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.PermitId).IsRequired();
        builder.Property(e => e.StartedAt);
        builder.Property(e => e.CompletedAt);
        builder.Property(e => e.ExecutionStatus).HasMaxLength(30).IsRequired();
        builder.Property(e => e.CompletionNotes);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_work_executions_tenant_id");
    }
}
