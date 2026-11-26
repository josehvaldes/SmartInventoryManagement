using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SmartInventory.Provisioning.Observability
{
    public sealed class Telemetry
    {
        public static string ActivitySourceName { get; } = "Provisioning.Api";
        public static readonly ActivitySource ActivitySource =
            new(ActivitySourceName);
    }
}
