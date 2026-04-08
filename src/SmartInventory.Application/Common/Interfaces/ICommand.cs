
using MediatR;

namespace SmartInventory.Application.Common.Interfaces
{
    public interface IBaseCommand { }

    public interface ICommand : IRequest<Unit>, IBaseCommand { }

    public interface ICommand<TResponse> : IRequest<TResponse>, IBaseCommand { }
}
