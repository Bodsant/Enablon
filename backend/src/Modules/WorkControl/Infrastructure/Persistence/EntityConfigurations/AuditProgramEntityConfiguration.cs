using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="AuditProgramEntity"/> (<c>audit.programs</c>).</summary>
public sealed class AuditProgramEntityConfiguration : IEntityTypeConfiguration<AuditProgramEntity>
{
    public const string TableName = "programs";

    public void Configure(EntityTypeBuilder<AuditProgramEntity> builder)
    {
        builder.ToTable(TableName, "audit");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.PeriodStart);
        builder.Property(e => e.PeriodEnd);
        builder.Property(e => e.OwnerMemberId).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(30).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_programs_tenant_id");
    }
}
