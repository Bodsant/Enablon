using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="IsolationVerificationEntity"/> (<c>cow.isolation_verifications</c>).</summary>
public sealed class IsolationVerificationEntityConfiguration : IEntityTypeConfiguration<IsolationVerificationEntity>
{
    public const string TableName = "isolation_verifications";

    public void Configure(EntityTypeBuilder<IsolationVerificationEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.IsolationPointId).IsRequired();
        builder.Property(e => e.VerificationType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Result).HasMaxLength(30).IsRequired();
        builder.Property(e => e.VerifiedByPersonId).IsRequired();
        builder.Property(e => e.VerifiedAt).IsRequired();
        builder.Property(e => e.Comment);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_isolation_verifications_tenant_id");
    }
}
