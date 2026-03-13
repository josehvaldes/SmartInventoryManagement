using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Infrastructure.Data.Configurations
{
    public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>, SmartInventoryDbContext.IInventoryConfiguration
    {
        public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
        {
            // ── Table & Schema ────────────────────────────────────────────────
            builder.ToTable("PurchaseOrders", "Purchasing");

            // ── Primary Key ───────────────────────────────────────────────────
            builder.HasKey(po => po.Id);

            builder.Property(po => po.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            // ── Order Number ──────────────────────────────────────────────────
            builder.Property(po => po.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(po => po.OrderNumber)
                .IsUnique()
                .HasDatabaseName("UK_PurchaseOrders_Number");

            // ── Dates ─────────────────────────────────────────────────────────
            builder.Property(po => po.OrderDate)
                .IsRequired();

            builder.Property(po => po.ExpectedDeliveryDate)
                .IsRequired();

            builder.Property(po => po.ActualDeliveryDate); // nullable

            builder.Property(po => po.ApprovedAt); // nullable

            // ── Enum ──────────────────────────────────────────────────────────
            builder.Property(po => po.Status)
                .IsRequired()
                .HasConversion<int>();

            // ── Monetary Amounts — all (18,2), not (18,4) ─────────────────────
            // These are currency values, not inventory quantities
            builder.Property(po => po.SubTotal)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0m);

            builder.Property(po => po.TaxAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0m);

            builder.Property(po => po.ShippingCost)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0m);

            builder.Property(po => po.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0m);

            // ── String Properties ─────────────────────────────────────────────
            builder.Property(po => po.Notes)
                .HasMaxLength(1000);

            builder.Property(po => po.ApprovedBy)
                .HasMaxLength(100);

            builder.Property(po => po.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(po => po.UpdatedBy)
                .IsRequired()
                .HasMaxLength(100);

            // ── Relationships ─────────────────────────────────────────────────

            // Supplier FK — no cascade, you don't want deleting a supplier
            // to wipe out historical purchase orders
            builder.HasOne<Supplier>()
                .WithMany()
                .HasForeignKey(po => po.SupplierId)
                .HasConstraintName("FK_PurchaseOrders_Suppliers")
                .OnDelete(DeleteBehavior.Restrict);

            // Warehouse FK — same reasoning as above
            builder.HasOne<Warehouse>()
                .WithMany()
                .HasForeignKey(po => po.WarehouseId)
                .HasConstraintName("FK_PurchaseOrders_Warehouses")
                .OnDelete(DeleteBehavior.Restrict);

            // Items — PurchaseOrderItems cannot exist without their parent PO.
            // Cascade delete matches ON DELETE CASCADE in your schema.
            builder.HasMany(po => po.Items)
                .WithOne()
                .HasForeignKey(item => item.PurchaseOrderId)
                .HasConstraintName("FK_PurchaseOrderItems_PurchaseOrders")
                .OnDelete(DeleteBehavior.Cascade);

            // ── Check Constraints ─────────────────────────────────────────────
            builder.ToTable(t =>
            {
                // Cross-column constraint — EF doesn't validate this in C#,
                // enforce in your domain entity or FluentValidation validator
                t.HasCheckConstraint("CK_PurchaseOrders_ExpectedDate",
                    "[ExpectedDeliveryDate] >= [OrderDate]");

                t.HasCheckConstraint("CK_PurchaseOrders_SubTotal", "[SubTotal] >= 0");
                t.HasCheckConstraint("CK_PurchaseOrders_TaxAmount", "[TaxAmount] >= 0");
                t.HasCheckConstraint("CK_PurchaseOrders_ShippingCost", "[ShippingCost] >= 0");
                t.HasCheckConstraint("CK_PurchaseOrders_TotalAmount", "[TotalAmount] >= 0");
            });

            // ── Timestamps ────────────────────────────────────────────────────
            builder.Property(po => po.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            // UpdatedAt is maintained by TR_PurchaseOrders_UpdatedAt trigger
            builder.Property(po => po.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            // ── Indexes ───────────────────────────────────────────────────────
            builder.HasIndex(po => po.SupplierId)
                .HasDatabaseName("IX_PurchaseOrders_SupplierId")
                .IncludeProperties(po => new { po.OrderDate, po.Status, po.TotalAmount });

            builder.HasIndex(po => po.WarehouseId)
                .HasDatabaseName("IX_PurchaseOrders_WarehouseId")
                .IncludeProperties(po => new { po.OrderDate, po.Status });

            builder.HasIndex(po => po.Status)
                .HasDatabaseName("IX_PurchaseOrders_Status")
                .IncludeProperties(po => new { po.OrderNumber, po.OrderDate, po.SupplierId });

            builder.HasIndex(po => po.OrderDate)
                .HasDatabaseName("IX_PurchaseOrders_OrderDate")
                .IsDescending()
                .IncludeProperties(po => new { po.OrderNumber, po.Status, po.TotalAmount });
        }
    }
}
