using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Identity;
using SmartInventory.Infrastructure.Data.Context;

namespace SmartInventory.Infrastructure.Data.Configurations
{
    public class UserConfiguration: IEntityTypeConfiguration<User>, AuthDbContext.IAuthConfiguration
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // ── Table & Schema ────────────────────────────────────────────────
            builder.ToTable("Users", "Auth");

            // ── Primary Key ───────────────────────────────────────────────────
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            // ── Properties ────────────────────────────────────────────────────
            // Your application layer (FluentValidation) enforces required fields
            // for registration/login flows, not the DB schema itself.
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // ── Indexes ───────────────────────────────────────────────────────
            // Email and Username are natural lookup keys for authentication —
            // unique indexes prevent duplicate accounts at the DB level
            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email")
                .HasFilter("[Email] IS NOT NULL"); // filtered: only index non-null emails

            builder.HasIndex(u => u.Username)
                .IsUnique()
                .HasDatabaseName("IX_Users_Username")
                .HasFilter("[Username] IS NOT NULL");
        }
    }
}
