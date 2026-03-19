using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Contracts.Responses.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Auth.Commands
{
    public record LoginCommand(string Username, string Password) : ICommand<LoginResponse>;

}
