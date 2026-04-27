using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Domain.Identity;

namespace SmartInventory.Infrastructure.Data.Context
{
    public class AuthDbContext : DbContext, IAuthDbContext
    {
        public interface IAuthConfiguration { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) 
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly,
                t => t.GetInterfaces().Contains(typeof(IAuthConfiguration))
            );
        }
    }
}
