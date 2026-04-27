using SmartInventory.Application.Common.Interfaces;
using MediatR;

namespace SmartInventory.Application.Features.Auth.Commands
{
    public record RevokeTokenCommand(string RefreshToken) : ICommand<Unit>;
}
