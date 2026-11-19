namespace SmartInventory.Application.Common.Exceptions
{
    /// <summary>The tenant could not be resolved or the request carries no valid tenant context.</summary>
    public class TenantException(string errorCode, string message, int statusCode) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;

        public int StatusCode { get; } = statusCode;
    }
}
