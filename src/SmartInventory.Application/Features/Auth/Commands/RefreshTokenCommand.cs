using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Contracts.Responses.Auth;

namespace SmartInventory.Application.Features.Auth.Commands
{
    public record RefreshTokenCommand(string RefreshToken) : ICommand<LoginResponse>;
}
