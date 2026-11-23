using MediatR;

namespace SmartInventory.Application.Common.Interfaces
{
    public interface IQueryHandler<TQuery, TResponse>: IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    { }
}
