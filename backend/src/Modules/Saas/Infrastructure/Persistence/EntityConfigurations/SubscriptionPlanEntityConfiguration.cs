using Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Saas.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="SubscriptionPlan"/> (<c>saas.subscription_plans</c>).</summary>
public sealed class SubscriptionPlanEntityConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public const string TableName = "subscription_plans";
    public const int CodeMaxLength = 30;
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 500;

    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable(TableName, "saas");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code).IsRequired().HasMaxLength(CodeMaxLength);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(NameMaxLength);
        builder.Property(e => e.Description).HasColumnType("text");
        builder.Property(e => e.IsActive).IsRequired();

        builder.HasIndex(e => e.Code).IsUnique().HasDatabaseName("ix_subscription_plans_code");
    }
}