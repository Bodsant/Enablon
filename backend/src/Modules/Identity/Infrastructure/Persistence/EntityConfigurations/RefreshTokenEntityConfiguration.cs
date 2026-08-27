using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Identity.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="RefreshTokenEntity"/> (<c>iam.refresh_tokens</c>).</summary>
public sealed class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
{
    public const string TableName = "refresh_tokens";
    public const int TokenHashMaxLength = 255;

    public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
    {
        builder.ToTable(TableName, "iam");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.TokenHash).IsRequired().HasMaxLength(TokenHashMaxLength);
        builder.Property(e => e.ExpiresAt).IsRequired();
        builder.Property(e => e.RevokedAt);
        builder.Property(e => e.ReplacedByTokenId);

        builder.HasIndex(e => e.TokenHash).IsUnique().HasDatabaseName("ix_refresh_tokens_token_hash");
        builder.HasIndex(e => e.UserId).HasDatabaseName("ix_refresh_tokens_user_id");
        builder.HasIndex(e => new { e.UserId, e.ExpiresAt }).HasDatabaseName("ix_refresh_tokens_user_id_expires_at");

        builder.HasOne(e => e.User)
            .WithMany(e => e.RefreshTokens)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReplacedByToken)
            .WithMany(e => e.ReplacedTokens)
            .HasForeignKey(e => e.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}