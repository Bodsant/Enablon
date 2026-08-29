using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="AuditResponseEntity"/> (<c>audit.responses</c>).</summary>
public sealed class AuditResponseEntityConfiguration : IEntityTypeConfiguration<AuditResponseEntity>
{
    public const string TableName = "responses";

    public void Configure(EntityTypeBuilder<AuditResponseEntity> builder)
    {
        builder.ToTable(TableName, "audit");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.AuditId).IsRequired();
        builder.Property(e => e.ChecklistItemId).IsRequired();
        builder.Property(e => e.Response).HasMaxLength(30);
        builder.Property(e => e.Comment);
        builder.Property(e => e.AuditorMemberId).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_responses_tenant_id");
    }
}
