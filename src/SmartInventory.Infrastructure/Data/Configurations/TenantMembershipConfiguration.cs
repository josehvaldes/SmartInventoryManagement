using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Identity;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Infrastructure.Data.Configurations
{
    public class TenantMembershipConfiguration : IEntityTypeConfiguration<TenantMembership>, AuthDbContext.IAuthConfiguration
    {
        public void Configure(EntityTypeBuilder<TenantMembership> builder)
        {
            builder.ToTable("TenantMemberships", "Auth");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(m => m.TenantId)
                .IsRequired();

            builder.Property(m => m.UserId)
                .IsRequired();
            
            builder.Property(t => t.Role)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(m => m.JoinedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(m => m.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(m => m.TenantId)
                .HasConstraintName("FK_TenantMemberships_Tenants")
                .OnDelete(DeleteBehavior.Cascade);
                        
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .HasConstraintName("FK_TenantMemberships_Users")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(m => new { m.TenantId, m.UserId })
                .IsUnique()
                .HasDatabaseName("IX_TenantMemberships_TenantId_UserId");

            builder.HasIndex(m => m.UserId)
                .HasDatabaseName("IX_TenantMemberships_UserId");
        }
    }
}
