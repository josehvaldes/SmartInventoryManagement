using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;

namespace SmartInventory.Infrastructure.Data.Configurations
{
    public class StockConfiguration : IEntityTypeConfiguration<Stock>, SmartInventoryDbContext.IInventoryConfiguration
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            // ── Table & Schema ────────────────────────────────────────────────
            builder.ToTable("Stock", "Inventory");

            // ── Primary Key ───────────────────────────────────────────────────
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            // ── Quantities ────────────────────────────────────────────────────
            builder.Property(s => s.QuantityOnHand)
                .IsRequired()
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0m);

            builder.Property(s => s.QuantityReserved)
                .IsRequired()
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0m);

            // QuantityAvailable is a PERSISTED computed column in SQL Server:
            //   AS ([QuantityOnHand] - [QuantityReserved]) PERSISTED
            // ValueGeneratedOnAddOrUpdate tells EF: never include this in INSERT/UPDATE,
            // always read it back from the database.
            builder.Property(s => s.QuantityAvailable)
                .HasColumnType("decimal(18,4)")
                .HasComputedColumnSql("[QuantityOnHand] - [QuantityReserved]", stored: true);

            // ── Timestamps & References ───────────────────────────────────────
            builder.Property(s => s.LastStockTakeDate); // nullable DateTime, no extra config needed

            builder.Property(s => s.LastUpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(s => s.LastTransactionId); // nullable Guid, no extra config needed

            // ── Unique Constraint: one Stock row per Product+Warehouse ─────────
            builder.HasIndex(s => new { s.ProductId, s.WarehouseId })
                .IsUnique()
                .HasDatabaseName("UK_Stock_Product_Warehouse");

            // ── Relationships ─────────────────────────────────────────────────
            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(s => s.ProductId)
                .HasConstraintName("FK_Stock_Products")
                .OnDelete(DeleteBehavior.Restrict); // Prevent accidental cascade delete

            builder.HasOne<Warehouse>()
                .WithMany()
                .HasForeignKey(s => s.WarehouseId)
                .HasConstraintName("FK_Stock_Warehouses")
                .OnDelete(DeleteBehavior.Restrict);

            // ── Check Constraints ─────────────────────────────────────────────
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Stock_QuantityOnHand", "[QuantityOnHand] >= 0");
                t.HasCheckConstraint("CK_Stock_QuantityReserved", "[QuantityReserved] >= 0");
                t.HasCheckConstraint("CK_Stock_Reserved_LTE_OnHand", "[QuantityReserved] <= [QuantityOnHand]");
            });

            // ── Indexes ───────────────────────────────────────────────────────
            builder.HasIndex(s => s.ProductId)
                .HasDatabaseName("IX_Stock_ProductId")
                .IncludeProperties(s => new { s.WarehouseId, s.QuantityOnHand, s.QuantityAvailable });

            builder.HasIndex(s => s.WarehouseId)
                .HasDatabaseName("IX_Stock_WarehouseId")
                .IncludeProperties(s => new { s.ProductId, s.QuantityOnHand, s.QuantityAvailable });
        }
    }
}
