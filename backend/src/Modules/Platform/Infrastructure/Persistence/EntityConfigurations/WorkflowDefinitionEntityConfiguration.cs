using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="WorkflowDefinitionEntity"/> (<c>platform.workflow_definitions</c>).</summary>
public sealed class WorkflowDefinitionEntityConfiguration : IEntityTypeConfiguration<WorkflowDefinitionEntity>
{
    public const string TableName = "workflow_definitions";
    public const int CodeMaxLength = 60;
    public const int NameMaxLength = 150;
    public const int ModuleCodeMaxLength = 40;
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<WorkflowDefinitionEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(CodeMaxLength);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(NameMaxLength);
        builder.Property(e => e.ModuleCode).IsRequired().HasMaxLength(ModuleCodeMaxLength);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_workflow_definitions_tenant_id");
        builder.HasIndex(e => new { e.TenantId, e.Code }).HasDatabaseName("ix_workflow_definitions_tenant_id_code");
    }
}