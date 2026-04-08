using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartInventory.Application.Common.Behaviors;
using SmartInventory.Application.Common.Validation;
using SmartInventory.Application.Features.Products.Queries.GetProducts;

namespace SmartInventory.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services,
            IConfiguration config)
        {

            // Scan the Application assembly where all handlers live
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(GetProductsQuery).Assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
                cfg.LicenseKey = config["MediatR:LicenseKey"] ?? "FREE-LIMITED-USE";
            });

            services.AddValidatorsFromAssembly(typeof(ValidationBehavior<,>).Assembly);

            return services;
        }
    }
}
