using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="RecordEntity"/> (<c>platform.records</c>).</summary>
public sealed class RecordEntityConfiguration : IEntityTypeConfiguration<RecordEntity>
{
    public const string TableName = "records";
    public const int ModuleCodeMaxLength = 40;
    public const int RecordTypeMaxLength = 60;
    public const int RecordNumberMaxLength = 60;
    public const int StatusMaxLength = 40;
    public const int TitleMaxLength = 250;

    public void Configure(EntityTypeBuilder<RecordEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ModuleCode).IsRequired().HasMaxLength(ModuleCodeMaxLength);
        builder.Property(e => e.RecordType).IsRequired().HasMaxLength(RecordTypeMaxLength);
        builder.Property(e => e.RecordNumber).IsRequired().HasMaxLength(RecordNumberMaxLength);
        builder.Property(e => e.CompanyId);
        builder.Property(e => e.BusinessUnitId);
        builder.Property(e => e.SiteId);
        builder.Property(e => e.DepartmentId);
        builder.Property(e => e.LocationId);
        builder.Property(e => e.DataClassificationId).IsRequired();
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);
        builder.Property(e => e.Title).HasMaxLength(TitleMaxLength);
        builder.Property(e => e.CreatedByMemberId).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.ArchivedAt);
        builder.Property(e => e.VoidedAt);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_records_tenant_id");
        builder.HasIndex(e => new { e.TenantId, e.ModuleCode }).HasDatabaseName("ix_records_tenant_id_module_code");
        builder.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("ix_records_tenant_id_status");
        builder.HasIndex(e => new { e.TenantId, e.RecordNumber }).HasDatabaseName("ix_records_tenant_id_record_number");
        builder.HasIndex(e => e.DataClassificationId).HasDatabaseName("ix_records_data_classification_id");

        builder.HasOne(e => e.DataClassification)
            .WithMany()
            .HasForeignKey(e => e.DataClassificationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}