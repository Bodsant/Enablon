using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="InspectionResponseEntity"/> (<c>inspection.responses</c>).</summary>
public sealed class InspectionResponseEntityConfiguration : IEntityTypeConfiguration<InspectionResponseEntity>
{
    public const string TableName = "responses";

    public void Configure(EntityTypeBuilder<InspectionResponseEntity> builder)
    {
        builder.ToTable(TableName, "inspection");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.InspectionId).IsRequired();
        builder.Property(e => e.TemplateItemId).IsRequired();
        builder.Property(e => e.ResponseJson).HasColumnType("jsonb");
        builder.Property(e => e.ComplianceStatus).HasMaxLength(30);
        builder.Property(e => e.Score).HasPrecision(10, 2);
        builder.Property(e => e.Comment);
        builder.Property(e => e.AnsweredByMemberId).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_responses_tenant_id");
    }
}
