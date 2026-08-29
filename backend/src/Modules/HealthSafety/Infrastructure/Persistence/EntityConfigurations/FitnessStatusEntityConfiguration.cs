using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="FitnessStatusEntity"/> (<c>health.fitness_statuses</c>).</summary>
public sealed class FitnessStatusEntityConfiguration : IEntityTypeConfiguration<FitnessStatusEntity>
{
    public const string TableName = "fitness_statuses";
    public const int FitnessStatusMaxLength = 40;

    public void Configure(EntityTypeBuilder<FitnessStatusEntity> builder)
    {
        builder.ToTable(TableName, "health");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.HealthProfileId).IsRequired();
        builder.Property(e => e.FitnessStatus).IsRequired().HasMaxLength(40);
        builder.Property(e => e.ValidFrom);
        builder.Property(e => e.ValidUntil);
        builder.Property(e => e.RestrictionsSummary);
        builder.Property(e => e.IssuedByMemberId);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_fitness_statuses_tenant_id");
        builder.HasIndex(e => e.HealthProfileId).HasDatabaseName("ix_fitness_statuses_health_profile_id");

    }
}
