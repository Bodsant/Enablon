using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="CapaVerificationEntity"/> (<c>capa.verifications</c>).</summary>
public sealed class CapaVerificationEntityConfiguration : IEntityTypeConfiguration<CapaVerificationEntity>
{
    public const string TableName = "verifications";

    public void Configure(EntityTypeBuilder<CapaVerificationEntity> builder)
    {
        builder.ToTable(TableName, "capa");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ActionId).IsRequired();
        builder.Property(e => e.VerifierMemberId).IsRequired();
        builder.Property(e => e.Result).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Comment);
        builder.Property(e => e.VerifiedAt).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_verifications_tenant_id");
    }
}
