using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Infrastructure.Data.Configurations
{
    public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>, SmartInventoryDbContext.IInventoryConfiguration
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
        {
            // ── Table & Schema ────────────────────────────────────────────────
            builder.ToTable("PurchaseOrderItems", "Purchasing");

            // ── Primary Key ───────────────────────────────────────────────────
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            // ── Composite Unique Constraint ───────────────────────────────────
            // A product can only appear once per purchase order
            builder.HasIndex(i => new { i.PurchaseOrderId, i.ProductId })
                .IsUnique()
                .HasDatabaseName("UK_PurchaseOrderItems_PO_Product");

            // ── Quantities & Costs — all (18,4) matching inventory precision ──
            builder.Property(i => i.Quantity)
                .IsRequired()
                .HasColumnType("decimal(18,4)");

            builder.Property(i => i.UnitCost)
                .IsRequired()
                .HasColumnType("decimal(18,4)");

            // TotalCost is calculated in the application layer (Quantity * UnitCost)
            // and persisted — it is NOT a computed column in SQL, so EF writes it normally
            builder.Property(i => i.TotalCost)
                .IsRequired()
                .HasColumnType("decimal(18,4)");

            builder.Property(i => i.ReceivedQuantity)
                .IsRequired()
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0m);

            // ── String Properties ─────────────────────────────────────────────
            builder.Property(i => i.Notes)
                .HasMaxLength(500);

            // ── Product FK ────────────────────────────────────────────────────
            // The PurchaseOrderId FK and cascade delete are already configured
            // in PurchaseOrderConfiguration via HasMany — don't redeclare it here.
            // Only the Product FK needs to be declared from this side.
            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .HasConstraintName("FK_PurchaseOrderItems_Products")
                .OnDelete(DeleteBehavior.Restrict);

            // ── Check Constraints ─────────────────────────────────────────────
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_PurchaseOrderItems_Quantity",
                    "[Quantity] > 0");

                t.HasCheckConstraint("CK_PurchaseOrderItems_UnitCost",
                    "[UnitCost] >= 0");

                // Cross-column constraint: received can't exceed ordered
                t.HasCheckConstraint("CK_PurchaseOrderItems_ReceivedQuantity",
                    "[ReceivedQuantity] >= 0 AND [ReceivedQuantity] <= [Quantity]");
            });

            // ── Indexes ───────────────────────────────────────────────────────
            builder.HasIndex(i => i.PurchaseOrderId)
                .HasDatabaseName("IX_PurchaseOrderItems_PurchaseOrderId")
                .IncludeProperties(i => new { i.ProductId, i.Quantity, i.UnitCost });

            builder.HasIndex(i => i.ProductId)
                .HasDatabaseName("IX_PurchaseOrderItems_ProductId")
                .IncludeProperties(i => new { i.PurchaseOrderId, i.Quantity, i.UnitCost });
        }
    }
}
