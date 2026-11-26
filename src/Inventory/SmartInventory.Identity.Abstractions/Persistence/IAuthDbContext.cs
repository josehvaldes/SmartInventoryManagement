using Microsoft.EntityFrameworkCore;
using SmartInventory.Domain.Identity;
using System.Data;

namespace SmartInventory.Identity.Abstractions.Persistence
{
    public interface IAuthDbContext : IAuthUnitOfWork
    {
        DbSet<User> Users { get; }
        DbSet<Role> Roles { get; }
        DbSet<UserRole> UserRoles { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<Tenant> Tenants { get; }
        DbSet<TenantMembership> TenantMemberships { get; }

    }
}
