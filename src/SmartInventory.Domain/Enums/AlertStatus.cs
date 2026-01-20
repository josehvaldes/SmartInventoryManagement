using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Enums
{
    /// <summary>
    /// Allows specification of the current status of an alert.
    /// </summary>
    public enum AlertStatus
    {
        New = 1,            // Just created
        Acknowledged = 2,   // Seen by user
        InProgress = 3,     // Being addressed
        Resolved = 4,       // Issue resolved
        Ignored = 5         // Marked as non-issue
    }
}
