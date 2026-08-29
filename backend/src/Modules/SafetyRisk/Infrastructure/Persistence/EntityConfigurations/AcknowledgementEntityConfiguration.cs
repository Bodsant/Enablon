using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="AcknowledgementEntity"/> (<c>document.acknowledgements</c>).</summary>
public sealed class AcknowledgementEntityConfiguration : IEntityTypeConfiguration<AcknowledgementEntity>
{
    public const string TableName = "acknowledgements";

    public void Configure(EntityTypeBuilder<AcknowledgementEntity> builder)
    {
        builder.ToTable(TableName, "document");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.DocumentRevisionId).IsRequired();
        builder.Property(e => e.TenantMemberId).IsRequired();
        builder.Property(e => e.AcknowledgedAt).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_acknowledgements_tenant_id");
    }
}
