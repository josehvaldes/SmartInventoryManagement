using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SmartInventory.Observability.Traces
{
    public static class Telemetry
    {
        public static string ActivitySourceName { get; } = "SmartInventory.Api";
        public static readonly ActivitySource ActivitySource =
            new(ActivitySourceName);


    }
}
