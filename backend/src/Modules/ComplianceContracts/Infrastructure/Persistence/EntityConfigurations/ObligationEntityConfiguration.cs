using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ObligationEntity"/> (<c>compliance.obligations</c>).</summary>
public sealed class ObligationEntityConfiguration : IEntityTypeConfiguration<ObligationEntity>
{
    public const string TableName = "obligations";

    public void Configure(EntityTypeBuilder<ObligationEntity> builder)
    {
        builder.ToTable(TableName, "compliance");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.LegalSourceVersionId).IsRequired();
        builder.Property(e => e.ClauseReference).HasMaxLength(150);
        builder.Property(e => e.RequirementText).IsRequired();
        builder.Property(e => e.OwnerMemberId).IsRequired();
        builder.Property(e => e.Frequency).HasMaxLength(80);
        builder.Property(e => e.DueDate);
        builder.Property(e => e.LastReview);
        builder.Property(e => e.NextReview);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_obligations_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_obligations_record_id");
        builder.HasIndex(e => e.LegalSourceVersionId).HasDatabaseName("ix_obligations_legal_source_version_id");
        builder.HasIndex(e => e.OwnerMemberId).HasDatabaseName("ix_obligations_owner_member_id");
    }
}
