using MediatR;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Identity.Abstractions.Persistence;

namespace SmartInventory.Application.Common.Behaviors
{
    public class UnitOfWorkBehavior<TRequest, TResponse>(
        IEnumerable<IUnitOfWork> unitOfWorks,
        IEnumerable<IAuthUnitOfWork> authUnitOfWorks)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (request is not IBaseCommand)
                return await next();

            var response = await next();
            foreach (var unitOfWork in unitOfWorks)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            foreach (var authUnitOfWork in authUnitOfWorks)
            {
                await authUnitOfWork.SaveChangesAsync(cancellationToken);
            }
            return response;
        }
    }
}
