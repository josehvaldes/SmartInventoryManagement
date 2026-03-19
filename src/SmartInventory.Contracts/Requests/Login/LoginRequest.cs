using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Contracts.Requests.Login
{
    public class LoginRequest
    {
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
