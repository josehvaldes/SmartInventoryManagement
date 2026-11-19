using Microsoft.Extensions.Options;
using SmartInventory.API.Settings;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Tenants;
using System.Net;

namespace SmartInventory.API.Utils
{
    public static class TenantUtils
    {
        public static string? GetSlug(HttpContext context, TenantResolutionSettings tenantResolutionSettings)
        {
            var baseDomain = tenantResolutionSettings.BaseDomain;

            // Middleware logic to resolve tenant goes here
            var domain = context.Request.Host.Host;

            // Get the tenant slug from the headers if the request is to the base domain or an IP address
            string? slug = GetSlugFromHeaders(context, tenantResolutionSettings);
            if (baseDomain.CompareTo(domain, StringComparison.OrdinalIgnoreCase) != 0)
            {
                // If the request is to the base domain or an IP address, check for a tenant slug in the headers
                var slugDomain = domain.EndsWith("." + baseDomain, StringComparison.OrdinalIgnoreCase) ? domain.Substring(0, domain.Length - baseDomain.Length - 1) : null;
                if (!string.IsNullOrEmpty(slugDomain))
                {
                    if (!string.IsNullOrEmpty(slug) && string.Compare(slug, slugDomain, StringComparison.OrdinalIgnoreCase) != 0) 
                    {
                        throw new TenantException(TenantErrorCodes.Mismatch, $"Tenant slug in header '{slug}' does not match the domain slug '{slugDomain}'.", StatusCodes.Status400BadRequest);
                    }

                    slug = slugDomain;
                }
            }

            return slug?.Trim().ToLowerInvariant();
        }

        public static string? GetSlugFromHeaders(HttpContext context, TenantResolutionSettings tenantResolutionSettings)
        {
            var headername = tenantResolutionSettings.SlugHeaderName;
            if (context.Request.Headers.TryGetValue(headername, out var headerSlug))
            {
                return headerSlug.ToString();
            }
            return null;
        }
    }
}
