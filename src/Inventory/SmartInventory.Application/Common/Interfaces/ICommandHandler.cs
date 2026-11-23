
using MediatR;

namespace SmartInventory.Application.Common.Interfaces
{
    public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Unit> where TCommand : ICommand
    {
    }

    public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
    { 
    }
}
