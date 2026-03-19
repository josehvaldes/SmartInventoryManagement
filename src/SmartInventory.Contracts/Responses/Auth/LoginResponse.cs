using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Contracts.Responses.Auth
{
    public class LoginResponse(string token, string username)
    {
        public string Token { get; } = token;
        public string Username { get; } = username;
    }
}
