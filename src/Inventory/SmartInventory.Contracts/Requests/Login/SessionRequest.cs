using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Contracts.Requests.Login
{
    public class SessionRequest
    {
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
