using Prometheus;

namespace SmartInventory.Observability.AppMetrics
{
    public static class WebMetrics
    {
        public static readonly Counter AuthenticationErrors = Metrics.CreateCounter(
            "smartinventory_authentication_errors_total",
            "Total authentication errors encountered");
        public static readonly Counter ServiceUnavailableErrors = Metrics.CreateCounter(
            "smartinventory_service_unavailable_errors_total",
            "Total service unavailable errors encountered");
    }
}
