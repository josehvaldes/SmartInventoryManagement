using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Infrastructure.Data.Configurations
{
    public class StockAlertConfiguration : IEntityTypeConfiguration<StockAlert>, SmartInventoryDbContext.IInventoryConfiguration
    {
        public void Configure(EntityTypeBuilder<StockAlert> builder)
        {
            // ── Table & Schema ────────────────────────────────────────────────
            builder.ToTable("StockAlerts", "Alerts");

            // ── Primary Key ───────────────────────────────────────────────────
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            // ── Enums ─────────────────────────────────────────────────────────
            builder.Property(a => a.AlertType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(a => a.Severity)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(a => a.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(AlertStatus.New)
                .HasSentinel(0);

            // ── Quantities ────────────────────────────────────────────────────
            builder.Property(a => a.CurrentQuantity)
                .IsRequired()
                .HasColumnType("decimal(18,4)");

            builder.Property(a => a.ThresholdQuantity)
                .IsRequired()
                .HasColumnType("decimal(18,4)");

            // ── Message ───────────────────────────────────────────────────────
            builder.Property(a => a.Message)
                .IsRequired()
                .HasMaxLength(500);

            // ── Lifecycle Columns — set by application, not triggers ──────────
            // No UpdatedAt here — alert state changes are tracked through
            // these dedicated nullable columns instead
            builder.Property(a => a.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(a => a.AcknowledgedAt); // nullable DateTime

            builder.Property(a => a.AcknowledgedBy)
                .HasMaxLength(100);

            builder.Property(a => a.ResolvedAt); // nullable DateTime

            builder.Property(a => a.ResolvedBy)
                .HasMaxLength(100);

            builder.Property(a => a.ResolutionNotes)
                .HasMaxLength(500);

            // ── Relationships ─────────────────────────────────────────────────
            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(a => a.ProductId)
                .HasConstraintName("FK_StockAlerts_Products")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Warehouse>()
                .WithMany()
                .HasForeignKey(a => a.WarehouseId)
                .HasConstraintName("FK_StockAlerts_Warehouses")
                .OnDelete(DeleteBehavior.Restrict);

            // ── Indexes ───────────────────────────────────────────────────────
            builder.HasIndex(a => a.ProductId)
                .HasDatabaseName("IX_StockAlerts_ProductId")
                .IncludeProperties(a => new { a.Status, a.Severity, a.CreatedAt });

            builder.HasIndex(a => a.WarehouseId)
                .HasDatabaseName("IX_StockAlerts_WarehouseId")
                .IncludeProperties(a => new { a.Status, a.Severity, a.CreatedAt });

            builder.HasIndex(a => a.Status)
                .HasDatabaseName("IX_StockAlerts_Status")
                .IncludeProperties(a => new { a.ProductId, a.WarehouseId, a.Severity, a.CreatedAt });

            builder.HasIndex(a => a.Severity)
                .HasDatabaseName("IX_StockAlerts_Severity")
                .IncludeProperties(a => new { a.Status, a.ProductId, a.CreatedAt });

            builder.HasIndex(a => a.CreatedAt)
                .HasDatabaseName("IX_StockAlerts_CreatedAt")
                .IsDescending()
                .IncludeProperties(a => new { a.Status, a.Severity, a.ProductId });
        }
    }
}
