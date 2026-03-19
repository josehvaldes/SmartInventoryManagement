using SmartInventory.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user, IEnumerable<string> roles);
    }
}
