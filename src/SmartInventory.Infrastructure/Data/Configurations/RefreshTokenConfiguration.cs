using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Identity;
using SmartInventory.Infrastructure.Data.Context;

namespace SmartInventory.Infrastructure.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>, AuthDbContext.IAuthConfiguration
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens", "Auth");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(rt => rt.Token)
                .IsUnique();

            builder.Property(rt => rt.ExpiresAt).IsRequired();
            builder.Property(rt => rt.CreatedAt).IsRequired();
            builder.Property(rt => rt.IsRevoked).IsRequired();

            builder.Property(rt => rt.ReplacedByToken)
                .HasMaxLength(256);

            builder.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
