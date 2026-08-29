using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="CourseEntity"/> (<c>training.courses</c>).</summary>
public sealed class CourseEntityConfiguration : IEntityTypeConfiguration<CourseEntity>
{
    public const string TableName = "courses";

    public void Configure(EntityTypeBuilder<CourseEntity> builder)
    {
        builder.ToTable(TableName, "training");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.ValidityMonths);
        builder.Property(e => e.ProviderType).HasMaxLength(40);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_courses_tenant_id");
    }
}
