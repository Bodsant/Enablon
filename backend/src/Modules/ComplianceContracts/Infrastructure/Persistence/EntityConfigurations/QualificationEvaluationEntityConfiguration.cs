using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="QualificationEvaluationEntity"/> (<c>contractor.qualification_evaluations</c>).</summary>
public sealed class QualificationEvaluationEntityConfiguration : IEntityTypeConfiguration<QualificationEvaluationEntity>
{
    public const string TableName = "qualification_evaluations";

    public void Configure(EntityTypeBuilder<QualificationEvaluationEntity> builder)
    {
        builder.ToTable(TableName, "contractor");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.ContractorCompanyId).IsRequired();
        builder.Property(e => e.EvaluationType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Result).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Score).HasPrecision(10, 2);
        builder.Property(e => e.EvaluatedByMemberId).IsRequired();
        builder.Property(e => e.ValidUntil);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_qualification_evaluations_tenant_id");
        builder.HasIndex(e => e.RecordId).IsUnique().HasDatabaseName("ix_qualification_evaluations_record_id");
        builder.HasIndex(e => e.ContractorCompanyId).HasDatabaseName("ix_qualification_evaluations_contractor_company_id");
        builder.HasIndex(e => e.EvaluatedByMemberId).HasDatabaseName("ix_qualification_evaluations_evaluated_by_member_id");
    }
}
