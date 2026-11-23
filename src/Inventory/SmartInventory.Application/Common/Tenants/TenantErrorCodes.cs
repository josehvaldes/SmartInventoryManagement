namespace SmartInventory.Application.Common.Tenants
{
    /// <summary>
    /// Machine-readable tenant error codes returned to clients, with the client-safe message for each.
    /// The exception message carries the internal detail and is only logged, never returned.
    /// </summary>
    public static class TenantErrorCodes
    {
        public const string SlugInvalid = "tenant_slug_invalid";
        public const string NotFound = "tenant_not_found";
        public const string Inactive = "tenant_inactive";
        public const string ContextRequired = "tenant_context_required";
        public const string AccessDenied = "tenant_access_denied";
        public const string Mismatch = "tenant_mismatch";

        public const string Forbidden = "tenant_forbidden_operation";

        public static string GetPublicMessage(string errorCode) => errorCode switch
        {
            SlugInvalid => "The tenant identifier is not valid.",
            NotFound => "The requested tenant could not be found.",
            Inactive => "The requested tenant is not currently available.",
            ContextRequired => "A tenant context is required. Please sign in again.",
            AccessDenied => "You do not have access to this tenant.",
            Mismatch => "The requested tenant does not match your current session.",
            Forbidden => "You are not allowed to perform this operation.",
            _ => "A tenant error occurred."
        };
    }
}
