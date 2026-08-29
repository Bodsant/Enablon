using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="TrainingSessionEntity"/> (<c>training.sessions</c>).</summary>
public sealed class TrainingSessionEntityConfiguration : IEntityTypeConfiguration<TrainingSessionEntity>
{
    public const string TableName = "sessions";

    public void Configure(EntityTypeBuilder<TrainingSessionEntity> builder)
    {
        builder.ToTable(TableName, "training");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.CourseId).IsRequired();
        builder.Property(e => e.ProviderName).HasMaxLength(200);
        builder.Property(e => e.StartsAt);
        builder.Property(e => e.EndsAt);
        builder.Property(e => e.Capacity);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_sessions_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_sessions_record_id");
        builder.HasIndex(e => e.CourseId).HasDatabaseName("ix_sessions_course_id");
    }
}
