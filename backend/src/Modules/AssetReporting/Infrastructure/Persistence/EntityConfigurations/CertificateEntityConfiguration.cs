using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="CertificateEntity"/> (<c>asset.certificates</c>).</summary>
public sealed class CertificateEntityConfiguration : IEntityTypeConfiguration<CertificateEntity>
{
    public const string TableName = "certificates";
    public const int CertificateTypeMaxLength = 60;
    public const int CertificateNumberMaxLength = 100;
    public const int ResultMaxLength = 30;

    public void Configure(EntityTypeBuilder<CertificateEntity> builder)
    {
        builder.ToTable(TableName, "asset");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.AssetId).IsRequired();
        builder.Property(e => e.CertificateType).IsRequired().HasMaxLength(CertificateTypeMaxLength);
        builder.Property(e => e.CertificateNumber).HasMaxLength(CertificateNumberMaxLength);
        builder.Property(e => e.IssueDate);
        builder.Property(e => e.ExpiryDate);
        builder.Property(e => e.Result).HasMaxLength(ResultMaxLength);
        builder.Property(e => e.FileObjectId);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_certificates_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_certificates_record_id");
        builder.HasIndex(e => e.AssetId).HasDatabaseName("ix_certificates_asset_id");
        builder.HasIndex(e => e.ExpiryDate).HasDatabaseName("ix_certificates_expiry_date");

        builder.HasOne(e => e.Asset)
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}