using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="LookupValueEntity"/> (<c>platform.lookup_values</c>).</summary>
public sealed class LookupValueEntityConfiguration : IEntityTypeConfiguration<LookupValueEntity>
{
    public const string TableName = "lookup_values";
    public const int CategoryMaxLength = 80;
    public const int CodeMaxLength = 60;
    public const int LabelMaxLength = 150;
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<LookupValueEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Category).IsRequired().HasMaxLength(CategoryMaxLength);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(CodeMaxLength);
        builder.Property(e => e.Label).IsRequired().HasMaxLength(LabelMaxLength);
        builder.Property(e => e.EffectiveFrom);
        builder.Property(e => e.EffectiveTo);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);
        builder.Property(e => e.MetadataJson).HasColumnType("jsonb");

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_lookup_values_tenant_id");
        builder.HasIndex(e => new { e.TenantId, e.Category }).HasDatabaseName("ix_lookup_values_tenant_id_category");
        builder.HasIndex(e => new { e.TenantId, e.Category, e.Code }).HasDatabaseName("ix_lookup_values_tenant_category_code");
    }
}