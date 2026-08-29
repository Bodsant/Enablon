using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="ContractorDocumentEntity"/> (<c>contractor.documents</c>).</summary>
public sealed class ContractorDocumentEntityConfiguration : IEntityTypeConfiguration<ContractorDocumentEntity>
{
    public const string TableName = "documents";

    public void Configure(EntityTypeBuilder<ContractorDocumentEntity> builder)
    {
        builder.ToTable(TableName, "contractor");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ContractorCompanyId);
        builder.Property(e => e.ContractorWorkerId);
        builder.Property(e => e.DocumentType).IsRequired().HasMaxLength(60);
        builder.Property(e => e.DocumentNumber).HasMaxLength(100);
        builder.Property(e => e.FileObjectId).IsRequired();
        builder.Property(e => e.IssueDate);
        builder.Property(e => e.ExpiryDate);
        builder.Property(e => e.VerificationStatus).HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_documents_tenant_id");
        builder.HasIndex(e => e.ContractorCompanyId).HasDatabaseName("ix_documents_contractor_company_id");
        builder.HasIndex(e => e.ContractorWorkerId).HasDatabaseName("ix_documents_contractor_worker_id");
        builder.HasIndex(e => e.FileObjectId).HasDatabaseName("ix_documents_file_object_id");
    }
}
