using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Infrastructure.Data.Configurations
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>, SmartInventoryDbContext.IInventoryConfiguration
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            // ── Table & Schema ────────────────────────────────────────────────
            builder.ToTable("Suppliers", "Purchasing");

            // ── Primary Key ───────────────────────────────────────────────────
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            // ── Code ──────────────────────────────────────────────────────────
            builder.Property(s => s.Code)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(s => s.Code)
                .IsUnique()
                .HasDatabaseName("UK_Suppliers_Code");

            // ── Basic String Properties ───────────────────────────────────────
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.ContactPerson)
                .HasMaxLength(100);

            builder.Property(s => s.Email)
                .HasMaxLength(100);

            builder.Property(s => s.Phone)
                .HasMaxLength(20);

            builder.Property(s => s.PaymentTerms)
                .HasMaxLength(200);

            // ── Numeric Properties ────────────────────────────────────────────
            builder.Property(s => s.LeadTimeDays)
                .IsRequired()
                .HasDefaultValue(0);

            // MinimumOrderValue uses (18,2) — monetary value, not a quantity
            builder.Property(s => s.MinimumOrderValue)
                .HasColumnType("decimal(18,2)"); // nullable, no default needed

            // Rating uses (3,2): values like 3.00, 4.75 — range 1.00 to 5.00
            builder.Property(s => s.Rating)
                .IsRequired()
                .HasColumnType("decimal(3,2)")
                .HasDefaultValue(3.0m);

            // ── Address Value Object ──────────────────────────────────────────
            // Same pattern as WarehouseConfiguration — columns share the same
            // Address_* naming convention in both tables
            builder.OwnsOne(s => s.Address, address =>
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
                t.HasCheckConstraint("CK_Suppliers_Email",
                    "[Email] IS NULL OR [Email] LIKE '%_@__%.__%'");

                t.HasCheckConstraint("CK_Suppliers_LeadTimeDays",
                    "[LeadTimeDays] >= 0");

                t.HasCheckConstraint("CK_Suppliers_Rating",
                    "[Rating] >= 1.0 AND [Rating] <= 5.0");

                t.HasCheckConstraint("CK_Suppliers_MinimumOrderValue",
                    "[MinimumOrderValue] IS NULL OR [MinimumOrderValue] >= 0");
            });

            // ── Timestamps ────────────────────────────────────────────────────
            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            // UpdatedAt is maintained by TR_Suppliers_UpdatedAt trigger
            builder.Property(s => s.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            // ── Indexes ───────────────────────────────────────────────────────
            builder.HasIndex(s => s.IsActive)
                .HasDatabaseName("IX_Suppliers_IsActive")
                .IncludeProperties(s => new { s.Code, s.Name });

            builder.HasIndex(s => s.Rating)
                .HasDatabaseName("IX_Suppliers_Rating")
                .IsDescending(); // Matches DESC in your schema
        }
    }
}
