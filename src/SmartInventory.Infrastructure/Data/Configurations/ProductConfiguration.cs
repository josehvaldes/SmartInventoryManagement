using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;

namespace SmartInventory.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>, SmartInventoryDbContext.IInventoryConfiguration
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products", "Inventory");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            // ── SKU ───────────────────────────────────────────────────────────
            builder.Property(p => p.SKU)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(p => p.SKU)
                .IsUnique()
                .HasDatabaseName("UK_Products_SKU");

            // ── Basic String Properties ───────────────────────────────────────
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.UpdatedBy)
                .IsRequired()
                .HasMaxLength(100);

            // ── Enums ─────────────────────────────────────────────────────────
            // Stored as INT — matches your schema and is readable in SQL queries
            builder.Property(p => p.Category)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(p => p.UnitOfMeasure)
                .IsRequired()
                .HasConversion<int>();

            // ── Decimal Columns ───────────────────────────────────────────────
            // DECIMAL(18,4) matches your schema — conventions give (18,2)
            builder.Property(p => p.MinimumStockLevel)
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0);

            builder.Property(p => p.ReorderPoint)
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0);

            builder.Property(p => p.ReorderQuantity)
                .HasColumnType("decimal(18,4)");

            builder.Property(p => p.UnitCost)
                .HasColumnType("decimal(18,4)");  // nullable — no default needed

            // ── Check Constraints ─────────────────────────────────────────────
            // EF Core doesn't enforce these in C#, but they protect the DB directly
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Products_MinimumStockLevel", "[MinimumStockLevel] >= 0");
                t.HasCheckConstraint("CK_Products_ReorderPoint", "[ReorderPoint] >= 0");
                t.HasCheckConstraint("CK_Products_ReorderQuantity", "[ReorderQuantity] > 0");
                t.HasCheckConstraint("CK_Products_UnitCost", "[UnitCost] IS NULL OR [UnitCost] >= 0");
            });

            // ── Timestamps ────────────────────────────────────────────────────
            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(p => p.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            // ── Indexes ───────────────────────────────────────────────────────
            builder.HasIndex(p => p.Category)
                .HasDatabaseName("IX_Products_Category")
                .IncludeProperties(p => new { p.Name, p.IsActive });

            builder.HasIndex(p => p.IsActive)
                .HasDatabaseName("IX_Products_IsActive")
                .IncludeProperties(p => new { p.SKU, p.Name });

            builder.HasIndex(p => p.Name)
                .HasDatabaseName("IX_Products_Name");
        }
    }
}
