using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PermitApprovalEntity"/> (<c>cow.permit_approvals</c>).</summary>
public sealed class PermitApprovalEntityConfiguration : IEntityTypeConfiguration<PermitApprovalEntity>
{
    public const string TableName = "permit_approvals";

    public void Configure(EntityTypeBuilder<PermitApprovalEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.PermitId).IsRequired();
        builder.Property(e => e.WorkflowTaskId).IsRequired();
        builder.Property(e => e.ApprovalLevel).IsRequired();
        builder.Property(e => e.Decision).HasMaxLength(30);
        builder.Property(e => e.ApproverMemberId);
        builder.Property(e => e.DecidedAt);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_permit_approvals_tenant_id");
    }
}
