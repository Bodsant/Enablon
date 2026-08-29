using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PermitChecklistResponseEntity"/> (<c>cow.permit_checklist_responses</c>).</summary>
public sealed class PermitChecklistResponseEntityConfiguration : IEntityTypeConfiguration<PermitChecklistResponseEntity>
{
    public const string TableName = "permit_checklist_responses";

    public void Configure(EntityTypeBuilder<PermitChecklistResponseEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.PermitId).IsRequired();
        builder.Property(e => e.ChecklistItemId).IsRequired();
        builder.Property(e => e.ResponseJson).HasColumnType("jsonb");
        builder.Property(e => e.IsSatisfied);
        builder.Property(e => e.CheckedByMemberId);
        builder.Property(e => e.CheckedAt);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_permit_checklist_responses_tenant_id");
    }
}
