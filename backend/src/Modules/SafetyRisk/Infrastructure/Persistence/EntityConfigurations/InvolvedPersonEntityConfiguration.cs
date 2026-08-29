using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="InvolvedPersonEntity"/> (<c>incident.involved_people</c>).</summary>
public sealed class InvolvedPersonEntityConfiguration : IEntityTypeConfiguration<InvolvedPersonEntity>
{
    public const string TableName = "involved_people";

    public void Configure(EntityTypeBuilder<InvolvedPersonEntity> builder)
    {
        builder.ToTable(TableName, "incident");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.IncidentId).IsRequired();
        builder.Property(e => e.PersonId);
        builder.Property(e => e.ExternalPersonName).HasMaxLength(200);
        builder.Property(e => e.InvolvementType).IsRequired().HasMaxLength(40);
        builder.Property(e => e.InjuryClassificationId);
        builder.Property(e => e.LostWorkDays);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_involved_people_tenant_id");
    }
}
