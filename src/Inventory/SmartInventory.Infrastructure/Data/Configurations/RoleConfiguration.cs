using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Identity;
using SmartInventory.Infrastructure.Data.Context;

namespace SmartInventory.Infrastructure.Data.Configurations
{
    public class RoleConfiguration: IEntityTypeConfiguration<Role>, AuthDbContext.IAuthConfiguration
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            // ── Table & Schema ────────────────────────────────────────────────
            builder.ToTable("Roles", "Auth");

            // ── Primary Key ───────────────────────────────────────────────────
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            // ── Properties ────────────────────────────────────────────────────
            // Nullable in your schema — matches the DDL as-is.
            // You may want to tighten this to IsRequired() + unique index
            // once you add role seeding (Admin, Manager, Viewer)
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            // ── Indexes ───────────────────────────────────────────────────────
            // Role names should be unique — "Admin" should only exist once
            builder.HasIndex(r => r.Name)
                .IsUnique()
                .HasDatabaseName("IX_Roles_Name")
                .HasFilter("[Name] IS NOT NULL");
        }
    }
}
