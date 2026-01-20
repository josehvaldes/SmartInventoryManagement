using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Enums
{
    /// <summary>
    /// Specifies the severity level of an alert.
    /// </summary>
    /// <remarks>Use this enumeration to indicate the urgency or importance of an alert when reporting or
    /// handling system events. The severity levels range from informational (Low) to critical (Critical), allowing
    /// consumers to prioritize responses appropriately.</remarks>
    public enum AlertSeverity
    {
        Low = 1,        // FYI only
        Medium = 2,     // Should address soon
        High = 3,       // Address today
        Critical = 4    // Immediate action required
    }
}
