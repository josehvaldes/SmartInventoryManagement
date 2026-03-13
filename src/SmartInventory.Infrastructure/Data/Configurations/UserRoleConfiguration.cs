using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Identity;
using SmartInventory.Infrastructure.Data.Context;

namespace SmartInventory.Infrastructure.Data.Configurations
{
    public class UserRoleConfiguration
    : IEntityTypeConfiguration<UserRole>,
      AuthDbContext.IAuthConfiguration
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            // ── Table & Schema ────────────────────────────────────────────────
            builder.ToTable("UserRoles", "Auth");

            // ── Composite Primary Key ─────────────────────────────────────────
            // (UserId, RoleId) is the PK — matches PK_UserRoles in your schema.
            // No separate Id column exists on this table.
            builder.HasKey(ur => new { ur.UserId, ur.RoleId });

            // ── Payload Columns ───────────────────────────────────────────────
            // These are what make UserRoles a "join table with payload" rather
            // than a simple implicit many-to-many. EF Core requires an explicit
            // entity (UserRole) and configuration when extra columns are present.
            builder.Property(ur => ur.AssignedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(ur => ur.AssignedBy)
                .IsRequired()
                .HasMaxLength(100);

            // ── Relationships ─────────────────────────────────────────────────

            // Many UserRoles → one User
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .HasConstraintName("FK_UserRoles_Users")
                .OnDelete(DeleteBehavior.Cascade); // deleting a user removes their role assignments

            // Many UserRoles → one Role
            builder.HasOne<Role>()
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .HasConstraintName("FK_UserRoles_Roles")
                .OnDelete(DeleteBehavior.Restrict); // don't allow deleting a role that's in use

            // ── Indexes ───────────────────────────────────────────────────────
            // The schema has one explicit index — looking up all users in a role.
            // The PK clustered index already covers (UserId, RoleId) lookups.
            builder.HasIndex(ur => ur.RoleId)
                .HasDatabaseName("IX_UserRoles_RoleId")
                .IncludeProperties(ur => ur.UserId);
        }
    }
}
