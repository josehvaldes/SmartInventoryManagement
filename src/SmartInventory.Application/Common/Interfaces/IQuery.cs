
using MediatR;

namespace SmartInventory.Application.Common.Interfaces
{
    public interface IQuery<TResponse>: IRequest<TResponse>
    {
    }
}
