using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;

namespace SmartInventory.Infrastructure.Data.Configurations
{
    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>, SmartInventoryDbContext.IInventoryConfiguration
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            // ── Table & Schema ────────────────────────────────────────────────
            builder.ToTable("Warehouses", "Inventory");

            // ── Primary Key ───────────────────────────────────────────────────
            builder.HasKey(w => w.Id);

            builder.Property(w => w.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            // ── Code ──────────────────────────────────────────────────────────
            builder.Property(w => w.Code)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(w => w.Code)
                .IsUnique()
                .HasDatabaseName("UK_Warehouses_Code");

            // ── Basic Properties ──────────────────────────────────────────────
            builder.Property(w => w.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(w => w.ManagerName)
                .HasMaxLength(100);

            builder.Property(w => w.ManagerEmail)
                .HasMaxLength(100);

            builder.Property(w => w.ManagerPhone)
                .HasMaxLength(20);

            // ── Enum ──────────────────────────────────────────────────────────
            builder.Property(w => w.WarehouseType)
                .IsRequired()
                .HasConversion<int>();

            // ── Decimal ───────────────────────────────────────────────────────
            builder.Property(w => w.Capacity)
                .HasColumnType("decimal(18,4)"); // nullable, no default needed

            // ── Address Value Object ──────────────────────────────────────────
            // OwnsOne maps Address as columns in the same Warehouses table.
            // HasColumnName must match the Address_* column names in your schema.
            builder.OwnsOne(w => w.Address, address =>
            {
                address.Property(a => a.Street)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("Address_Street");

                address.Property(a => a.City)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("Address_City");

                address.Property(a => a.State)
                    .HasMaxLength(100)
                    .HasColumnName("Address_State");

                address.Property(a => a.PostalCode)
                    .HasMaxLength(20)
                    .HasColumnName("Address_PostalCode");

                address.Property(a => a.Country)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("Address_Country");
            });

            // ── Check Constraints ─────────────────────────────────────────────
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Warehouses_Capacity",
                    "[Capacity] IS NULL OR [Capacity] > 0");

                // Note: SQL LIKE pattern validation — EF won't enforce this in C#,
                // your FluentValidation validator should handle email format instead
                t.HasCheckConstraint("CK_Warehouses_Email",
                    "[ManagerEmail] IS NULL OR [ManagerEmail] LIKE '%_@__%.__%'");
            });

            // ── Timestamps ────────────────────────────────────────────────────
            builder.Property(w => w.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            // UpdatedAt is maintained by TR_Warehouses_UpdatedAt trigger in SQL.
            // HasDefaultValueSql here just stops EF from sending 0001-01-01 on INSERT.
            builder.Property(w => w.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            // ── Indexes ───────────────────────────────────────────────────────
            builder.HasIndex(w => w.IsActive)
                .HasDatabaseName("IX_Warehouses_IsActive")
                .IncludeProperties(w => new { w.Code, w.Name });

            builder.HasIndex(w => w.WarehouseType)
                .HasDatabaseName("IX_Warehouses_Type");
        }
    }
}
