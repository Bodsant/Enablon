using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="PositionRequirementEntity"/> (<c>training.position_requirements</c>).</summary>
public sealed class PositionRequirementEntityConfiguration : IEntityTypeConfiguration<PositionRequirementEntity>
{
    public const string TableName = "position_requirements";

    public void Configure(EntityTypeBuilder<PositionRequirementEntity> builder)
    {
        builder.ToTable(TableName, "training");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.PositionId).IsRequired();
        builder.Property(e => e.CompetencyId).IsRequired();
        builder.Property(e => e.CourseId);
        builder.Property(e => e.IsMandatory).IsRequired();
        builder.Property(e => e.MinimumLevel).HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_position_requirements_tenant_id");
        builder.HasIndex(e => e.PositionId).HasDatabaseName("ix_position_requirements_position_id");
        builder.HasIndex(e => e.CompetencyId).HasDatabaseName("ix_position_requirements_competency_id");
        builder.HasIndex(e => e.CourseId).HasDatabaseName("ix_position_requirements_course_id");
    }
}
