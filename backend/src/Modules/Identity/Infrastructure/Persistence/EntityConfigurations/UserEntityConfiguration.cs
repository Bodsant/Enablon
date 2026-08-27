using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="UserEntity"/> (<c>iam.users</c>).</summary>
public sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public const string TableName = "users";
    public const int EmailMaxLength = 254;
    public const int IdentityProviderMaxLength = 80;
    public const int ExternalSubjectMaxLength = 200;
    public const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable(TableName, "iam");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Email).IsRequired().HasMaxLength(EmailMaxLength);
        builder.Property(e => e.NormalizedEmail).IsRequired().HasMaxLength(EmailMaxLength);
        builder.Property(e => e.PasswordHash); // text, unbounded
        builder.Property(e => e.IdentityProvider).HasMaxLength(IdentityProviderMaxLength);
        builder.Property(e => e.ExternalSubject).HasMaxLength(ExternalSubjectMaxLength);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(StatusMaxLength);
        builder.Property(e => e.LastLoginAt);

        builder.HasIndex(e => e.Email).IsUnique().HasDatabaseName("ix_users_email");
        builder.HasIndex(e => e.NormalizedEmail).IsUnique().HasDatabaseName("ix_users_normalized_email");

        builder.HasMany(e => e.TenantMembers)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.RefreshTokens)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}