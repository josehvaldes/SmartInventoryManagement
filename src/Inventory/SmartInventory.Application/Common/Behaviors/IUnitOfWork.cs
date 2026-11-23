using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Common.Behaviors
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
