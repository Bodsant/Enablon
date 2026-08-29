using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="HealthProfileEntity"/> (<c>health.profiles</c>).</summary>
public sealed class HealthProfileEntityConfiguration : IEntityTypeConfiguration<HealthProfileEntity>
{
    public const string TableName = "profiles";

    public void Configure(EntityTypeBuilder<HealthProfileEntity> builder)
    {
        builder.ToTable(TableName, "health");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.PersonId).IsRequired();
        builder.Property(e => e.RestrictedIdentifier).HasMaxLength(100);
        builder.Property(e => e.DataClassificationId);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_profiles_tenant_id");
        builder.HasIndex(e => e.PersonId).IsUnique().HasDatabaseName("ix_profiles_person_id");

    }
}
