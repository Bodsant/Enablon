using Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="JsaStepHazardEntity"/> (<c>cow.jsa_step_hazards</c>).</summary>
public sealed class JsaStepHazardEntityConfiguration : IEntityTypeConfiguration<JsaStepHazardEntity>
{
    public const string TableName = "jsa_step_hazards";

    public void Configure(EntityTypeBuilder<JsaStepHazardEntity> builder)
    {
        builder.ToTable(TableName, "cow");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.JsaStepId).IsRequired();
        builder.Property(e => e.HazardId);
        builder.Property(e => e.Consequence).IsRequired();
        builder.Property(e => e.ExistingControl);
        builder.Property(e => e.AdditionalControl);
        builder.Property(e => e.InitialRiskLevel).HasMaxLength(30);
        builder.Property(e => e.ResidualRiskLevel).HasMaxLength(30);
        builder.Property(e => e.ResponsibleMemberId);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_jsa_step_hazards_tenant_id");
    }
}
