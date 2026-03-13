using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;

namespace SmartInventory.Infrastructure.Data.Configurations
{
    public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>, SmartInventoryDbContext.IInventoryConfiguration
    {
        public void Configure(EntityTypeBuilder<StockTransaction> builder)
        {
            // ── Table & Schema ────────────────────────────────────────────────
            builder.ToTable("StockTransactions", "Inventory");

            // ── Primary Key ───────────────────────────────────────────────────
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            // ── Transaction Number ────────────────────────────────────────────
            builder.Property(t => t.TransactionNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(t => t.TransactionNumber)
                .IsUnique()
                .HasDatabaseName("UK_StockTransactions_Number");

            // ── Enum ──────────────────────────────────────────────────────────
            builder.Property(t => t.TransactionType)
                .IsRequired()
                .HasConversion<int>();

            // ── Quantities & Costs ────────────────────────────────────────────
            builder.Property(t => t.Quantity)
                .IsRequired()
                .HasColumnType("decimal(18,4)");

            builder.Property(t => t.UnitCost)
                .HasColumnType("decimal(18,4)"); // nullable

            builder.Property(t => t.TotalCost)
                .HasColumnType("decimal(18,4)"); // nullable, computed in application layer

            // ── String Properties ─────────────────────────────────────────────
            builder.Property(t => t.ReferenceNumber)
                .HasMaxLength(100);

            builder.Property(t => t.Reason)
                .HasMaxLength(500);

            builder.Property(t => t.Notes)
                .HasMaxLength(1000);

            builder.Property(t => t.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            // ── Timestamps ────────────────────────────────────────────────────
            builder.Property(t => t.TransactionDate)
                .IsRequired();

            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            // ── Reversal Fields ───────────────────────────────────────────────
            builder.Property(t => t.IsReversed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(t => t.ReversedByTransactionId); // nullable Guid

            // ── Relationships ─────────────────────────────────────────────────

            // Product FK
            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(t => t.ProductId)
                .HasConstraintName("FK_StockTransactions_Products")
                .OnDelete(DeleteBehavior.Restrict);

            // Primary Warehouse FK
            builder.HasOne<Warehouse>()
                .WithMany()
                .HasForeignKey(t => t.WarehouseId)
                .HasConstraintName("FK_StockTransactions_Warehouses")
                .OnDelete(DeleteBehavior.Restrict);

            // SourceWarehouseId — optional, only used for Transfer transactions
            // Must use UsingEntity or explicit FK config to avoid EF picking the wrong navigation
            builder.HasOne<Warehouse>()
                .WithMany()
                .HasForeignKey(t => t.SourceWarehouseId)
                .HasConstraintName("FK_StockTransactions_SourceWarehouse")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // DestinationWarehouseId — optional, only used for Transfer transactions
            builder.HasOne<Warehouse>()
                .WithMany()
                .HasForeignKey(t => t.DestinationWarehouseId)
                .HasConstraintName("FK_StockTransactions_DestinationWarehouse")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Self-referencing FK — a reversal transaction points back to the original
            builder.HasOne<StockTransaction>()
                .WithMany()
                .HasForeignKey(t => t.ReversedByTransactionId)
                .HasConstraintName("FK_StockTransactions_ReversedBy")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // ── Check Constraints ─────────────────────────────────────────────
            builder.ToTable(t =>
            {
                // Quantity can never be zero — positive for receipts, negative for issues
                t.HasCheckConstraint("CK_StockTransactions_Quantity",
                    "[Quantity] <> 0");

                t.HasCheckConstraint("CK_StockTransactions_UnitCost",
                    "[UnitCost] IS NULL OR [UnitCost] >= 0");

                // Prevents future-dated transactions — enforced at DB level as a backstop
                // Your application layer should also validate this via FluentValidation
                t.HasCheckConstraint("CK_StockTransactions_TransactionDate",
                    "[TransactionDate] <= SYSUTCDATETIME()");
            });

            // ── Indexes ───────────────────────────────────────────────────────
            builder.HasIndex(t => t.ProductId)
                .HasDatabaseName("IX_StockTransactions_ProductId")
                .IncludeProperties(t => new { t.TransactionDate, t.Quantity, t.TransactionType });

            builder.HasIndex(t => t.WarehouseId)
                .HasDatabaseName("IX_StockTransactions_WarehouseId")
                .IncludeProperties(t => new { t.TransactionDate, t.ProductId, t.Quantity });

            builder.HasIndex(t => t.TransactionDate)
                .HasDatabaseName("IX_StockTransactions_Date")
                .IncludeProperties(t => new { t.ProductId, t.WarehouseId, t.Quantity, t.TransactionType });

            builder.HasIndex(t => t.TransactionType)
                .HasDatabaseName("IX_StockTransactions_Type")
                .IncludeProperties(t => new { t.ProductId, t.WarehouseId, t.TransactionDate });

            builder.HasIndex(t => t.ReferenceNumber)
                .HasDatabaseName("IX_StockTransactions_Reference")
                .HasFilter("[ReferenceNumber] IS NOT NULL"); // Filtered index — matches your schema
        }
    }
}
