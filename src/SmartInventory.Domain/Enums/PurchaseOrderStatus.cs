using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Enums
{
    /// <summary>
    /// Specifies the status of a purchase order in the procurement workflow.
    /// </summary>
    /// <remarks>Use this enumeration to track and manage the lifecycle of a purchase order, from creation
    /// through completion or cancellation. The status reflects the current state of the order and may influence
    /// available actions in the system.</remarks>
    public enum PurchaseOrderStatus
    {
        Draft = 1,          // Being created
        Submitted = 2,      // Sent to supplier
        Confirmed = 3,      // Supplier confirmed
        PartiallyReceived = 4, // Some items received
        Received = 5,       // Fully received
        Cancelled = 6,      // Order cancelled
        Closed = 7          // Administratively closed
    }
}
