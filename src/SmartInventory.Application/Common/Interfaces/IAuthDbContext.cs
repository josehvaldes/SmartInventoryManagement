using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Behaviors;
using SmartInventory.Domain.Identity;

namespace SmartInventory.Application.Common.Interfaces
{
    public interface IAuthDbContext: IUnitOfWork
    {
        DbSet<User> Users { get; }
        DbSet<Role> Roles { get; }
        DbSet<UserRole> UserRoles { get; }

    }
}
