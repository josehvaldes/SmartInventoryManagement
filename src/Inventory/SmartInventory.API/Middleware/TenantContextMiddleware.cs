using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SmartInventory.API.Services;
using SmartInventory.API.Settings;
using SmartInventory.API.Utils;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Common.Tenants;
using SmartInventory.Application.Features.Tenants.Queries;
using SmartInventory.Domain.Enums;

namespace SmartInventory.API.Middleware
{
    public class TenantContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantContextMiddleware> _logger;

        public TenantContextMiddleware(RequestDelegate next, ILogger<TenantContextMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, 
            TenantContext tenantContext, 
            IMediator mediator,
            ICurrentUserService userContext,
            IOptions<TenantResolutionSettings> tenantResolutionSettings) 
        {
            var endpoint = context.GetEndpoint();

            var allowsAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null;

            if (endpoint is null || allowsAnonymous)
            {
                await _next(context);
                return;
            }

            // READ the tenantID from JWT claims and set it in the TenantContext
            var tenantIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == TenantConstants.TenantIdClaimType);
            var roleClaim = context.User.Claims.FirstOrDefault(c => c.Type == TenantConstants.TenantRoleClaimType);
            var roleClaimValue = roleClaim?.Value ?? throw new TenantException(TenantErrorCodes.ContextRequired, "Tenant role claim is missing from the token.", StatusCodes.Status401Unauthorized);

            if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId) || tenantId == Guid.Empty)
            {
                _logger.LogWarning("Tenant ID claim missing or invalid. UserId: {UserId}", userContext.UserId);
                throw new TenantException(TenantErrorCodes.ContextRequired, "Tenant ID claim is missing or invalid.", StatusCodes.Status401Unauthorized);
            }

            var query = new GetTenantMembershipsQuery(tenantId, userContext.UserId);
            var membership = await mediator.Send(query, context.RequestAborted);

            if (membership == null)
            {
                throw new TenantException(TenantErrorCodes.AccessDenied, $"No membership for user {userContext.UserId} in tenant {tenantId}.", StatusCodes.Status403Forbidden);
            }

            var referedSlug = TenantUtils.GetSlug(context, tenantResolutionSettings.Value);
            if (!string.IsNullOrEmpty(referedSlug) && !string.Equals(referedSlug, membership.Tenant.Slug, StringComparison.OrdinalIgnoreCase))
            {
                throw new TenantException(TenantErrorCodes.Mismatch, $"Requested slug '{referedSlug}' does not match token tenant '{membership.Tenant.Slug}'.", StatusCodes.Status403Forbidden);
            }

            if (membership.Role.ToString() != roleClaimValue || !membership.IsActive )
            {
                throw new TenantException(TenantErrorCodes.AccessDenied, $"Membership of user {userContext.UserId} in tenant {tenantId} is inactive or its role differs from the token claim '{roleClaimValue}'.", StatusCodes.Status403Forbidden);
            }

            if (membership.Tenant.Status != TenantStatus.Active)
            {
                throw new TenantException(TenantErrorCodes.Inactive, $"Tenant {tenantId} has status {membership.Tenant.Status}.", StatusCodes.Status403Forbidden);
            }

            tenantContext.Initialize(tenantId, roleClaimValue);
            await _next(context);
        }
    }
}
