using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="CertificationEntity"/> (<c>training.certifications</c>).</summary>
public sealed class CertificationEntityConfiguration : IEntityTypeConfiguration<CertificationEntity>
{
    public const string TableName = "certifications";

    public void Configure(EntityTypeBuilder<CertificationEntity> builder)
    {
        builder.ToTable(TableName, "training");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.PersonId).IsRequired();
        builder.Property(e => e.CourseId);
        builder.Property(e => e.CertificateNumber).HasMaxLength(100);
        builder.Property(e => e.IssuedAt);
        builder.Property(e => e.ExpiresAt);
        builder.Property(e => e.FileObjectId);
        builder.Property(e => e.VerificationStatus).HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_certifications_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_certifications_record_id");
        builder.HasIndex(e => e.PersonId).HasDatabaseName("ix_certifications_person_id");
        builder.HasIndex(e => e.CourseId).HasDatabaseName("ix_certifications_course_id");
        builder.HasIndex(e => e.FileObjectId).HasDatabaseName("ix_certifications_file_object_id");
    }
}
