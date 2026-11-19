using System.Text.RegularExpressions;
using MediatR;
using Microsoft.Extensions.Options;
using SmartInventory.API.Services;
using SmartInventory.API.Settings;
using SmartInventory.API.Utils;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Tenants;
using SmartInventory.Application.Features.Tenants.Queries;
using SmartInventory.Domain.Enums;

namespace SmartInventory.API.Middleware
{
    public partial class TenantResolutionMiddleware
    {
        private const int MaxLoggedSlugLength = 50;

        [GeneratedRegex("^[a-z0-9](?:[a-z0-9-]{0,48}[a-z0-9])?$", RegexOptions.CultureInvariant)]
        private static partial Regex SlugRegex();

        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolutionMiddleware> _logger;

        public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, 
            IOptions<TenantResolutionSettings> tenantResolutionSettings, 
            IMediator mediator, TenantContext tenantContext)
        {

            var slug = TenantUtils.GetSlug(context, tenantResolutionSettings.Value);

            if (!string.IsNullOrEmpty(slug))
            {
                if (!SlugRegex().IsMatch(slug))
                {
                    // The slug is client-controlled and unvalidated here, so cap what gets logged.
                    var loggedSlug = slug.Length > MaxLoggedSlugLength ? slug[..MaxLoggedSlugLength] : slug;
                    _logger.LogWarning("Invalid tenant slug: {Slug}", loggedSlug);
                    throw new TenantException(TenantErrorCodes.SlugInvalid, "Tenant slug failed validation.", StatusCodes.Status400BadRequest);
                }

                var query = new GetTenantBySlugQuery(slug);
                var tenant = await mediator.Send(query, context.RequestAborted);
                if (tenant == null)
                {
                    _logger.LogWarning("Tenant not found for slug: {Slug}", slug);
                    throw new TenantException(TenantErrorCodes.NotFound, $"No tenant found for slug '{slug}'.", StatusCodes.Status404NotFound);
                }
                if (tenant.Status != TenantStatus.Active)
                {
                    _logger.LogWarning("Tenant with slug {Slug} is not active. Status: {Status}", slug, tenant.Status);
                    throw new TenantException(TenantErrorCodes.Inactive, $"Tenant '{slug}' has status {tenant.Status}.", StatusCodes.Status403Forbidden);
                }

                tenantContext.SetResolvedTenantId(tenant.Id);
            }
            else 
            {
                // No slug initialized. let the TenantContext use the tenantId with its default value
                _logger.LogDebug("No tenant slug found in the request. Using default tenant context.");
            }

            await _next(context);
        }


    }
}
