using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        string Username { get; }
    }
}
