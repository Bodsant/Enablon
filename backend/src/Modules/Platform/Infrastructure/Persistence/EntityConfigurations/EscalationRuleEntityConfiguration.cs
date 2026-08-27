using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="EscalationRuleEntity"/> (<c>platform.escalation_rules</c>).</summary>
public sealed class EscalationRuleEntityConfiguration : IEntityTypeConfiguration<EscalationRuleEntity>
{
    public const string TableName = "escalation_rules";
    public const int EventCodeMaxLength = 60;

    public void Configure(EntityTypeBuilder<EscalationRuleEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.WorkflowVersionId);
        builder.Property(e => e.EventCode).IsRequired().HasMaxLength(EventCodeMaxLength);
        builder.Property(e => e.ConditionJson).IsRequired().HasColumnType("jsonb");
        builder.Property(e => e.ActionJson).IsRequired().HasColumnType("jsonb");
        builder.Property(e => e.IsActive).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_escalation_rules_tenant_id");
        builder.HasIndex(e => e.WorkflowVersionId).HasDatabaseName("ix_escalation_rules_workflow_version_id");

        builder.HasOne(e => e.Version)
            .WithMany(e => e.EscalationRules)
            .HasForeignKey(e => e.WorkflowVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}