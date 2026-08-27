using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Platform.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="NumberSequenceEntity"/> (<c>platform.number_sequences</c>).</summary>
public sealed class NumberSequenceEntityConfiguration : IEntityTypeConfiguration<NumberSequenceEntity>
{
    public const string TableName = "number_sequences";
    public const int SequenceCodeMaxLength = 40;
    public const int PeriodKeyMaxLength = 20;

    public void Configure(EntityTypeBuilder<NumberSequenceEntity> builder)
    {
        builder.ToTable(TableName, "platform");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.SequenceCode).IsRequired().HasMaxLength(SequenceCodeMaxLength);
        builder.Property(e => e.PeriodKey).IsRequired().HasMaxLength(PeriodKeyMaxLength);
        builder.Property(e => e.CurrentValue).IsRequired();
        builder.Property(e => e.LockVersion).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_number_sequences_tenant_id");
        builder.HasIndex(e => new { e.TenantId, e.SequenceCode, e.PeriodKey }).HasDatabaseName("ix_number_sequences_tenant_code_period");
    }
}