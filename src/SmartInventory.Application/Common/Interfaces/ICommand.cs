
using MediatR;

namespace SmartInventory.Application.Common.Interfaces
{
    public interface ICommand : IRequest<Unit> { }

    public interface ICommand<TResponse> : IRequest<TResponse> { }
}
