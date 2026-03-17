using Microsoft.EntityFrameworkCore;
using SmartInventory.Domain.Identity;

namespace SmartInventory.Application.Common.Interfaces
{
    public interface IAuthDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Role> Roles { get; }
        DbSet<UserRole> UserRoles { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
