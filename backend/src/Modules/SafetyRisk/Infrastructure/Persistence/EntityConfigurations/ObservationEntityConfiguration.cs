using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ObservationEntity"/> (<c>safety.observations</c>).</summary>
public sealed class ObservationEntityConfiguration : IEntityTypeConfiguration<ObservationEntity>
{
    public const string TableName = "observations";

    public void Configure(EntityTypeBuilder<ObservationEntity> builder)
    {
        builder.ToTable(TableName, "safety");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.ObservationType).IsRequired().HasMaxLength(40);
        builder.Property(e => e.ReporterMemberId).IsRequired();
        builder.Property(e => e.ReporterVisibility).IsRequired().HasMaxLength(30);
        builder.Property(e => e.PotentialImpact);
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.ImmediateAction);
        builder.Property(e => e.InitialRiskLevel).HasMaxLength(30);
        builder.Property(e => e.AssignedMemberId);
        builder.Property(e => e.DueDate);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_observations_tenant_id");
    }
}
