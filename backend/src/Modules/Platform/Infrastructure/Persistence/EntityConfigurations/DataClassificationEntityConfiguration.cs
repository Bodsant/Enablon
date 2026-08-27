using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="DataClassificationEntity"/> (<c>platform.data_classifications</c>).</summary>
public sealed class DataClassificationEntityConfiguration : IEntityTypeConfiguration<DataClassificationEntity>
{
    public const string TableName = "data_classifications";
    public const int CodeMaxLength = 30;
    public const int NameMaxLength = 100;

    public void Configure(EntityTypeBuilder<DataClassificationEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(CodeMaxLength);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(NameMaxLength);
        builder.Property(e => e.Rank).IsRequired();
        builder.Property(e => e.IsRestricted).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_data_classifications_tenant_id");
    }
}