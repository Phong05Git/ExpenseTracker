using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Ignore(x => x.Token);

        builder.Property(x => x.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.RevokedAt);

        builder.Property(x => x.LastActivityAt)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}