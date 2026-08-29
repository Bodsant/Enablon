using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="WorkRequestEntity"/> (<c>cow.work_requests</c>).</summary>
public sealed class WorkRequestEntityConfiguration : IEntityTypeConfiguration<WorkRequestEntity>
{
    public const string TableName = "work_requests";

    public void Configure(EntityTypeBuilder<WorkRequestEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.RequesterMemberId).IsRequired();
        builder.Property(e => e.WorkDescription).IsRequired();
        builder.Property(e => e.ContractorCompanyId);
        builder.Property(e => e.PlannedStart);
        builder.Property(e => e.PlannedEnd);
        builder.Property(e => e.WorkType).HasMaxLength(60).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_work_requests_tenant_id");
    }
}
