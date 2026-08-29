using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="JsaStepEntity"/> (<c>cow.jsa_steps</c>).</summary>
public sealed class JsaStepEntityConfiguration : IEntityTypeConfiguration<JsaStepEntity>
{
    public const string TableName = "jsa_steps";

    public void Configure(EntityTypeBuilder<JsaStepEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.JsaId).IsRequired();
        builder.Property(e => e.SequenceNumber).IsRequired();
        builder.Property(e => e.WorkStep).IsRequired();

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_jsa_steps_tenant_id");
    }
}
