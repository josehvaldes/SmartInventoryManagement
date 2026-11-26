using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Identity.Abstractions.Persistence
{
    public interface IAuthUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
