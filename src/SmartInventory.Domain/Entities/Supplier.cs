using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Entities
{
    /// <summary>
    /// Supplier entity representing suppliers in the inventory system.
    /// 
    /// Id: Guid (Primary Key)
    /// Code: string (unique, max 20 chars, e.g., "SUP-001")
    /// Name: string (required, max 200 chars)
    /// ContactPerson: string (optional, max 100 chars)
    /// Email: string (optional, max 100 chars)
    /// Phone: string (optional, max 20 chars)
    /// Address: Address (value object)
    /// PaymentTerms: string (optional, max 200 chars, e.g., "Net 30")
    /// LeadTimeDays: int (default: 0, estimated delivery time)
    /// MinimumOrderValue: decimal? (optional)
    /// Rating: decimal (1.0 to 5.0, default: 3.0)
    /// IsActive: bool (default: true)
    /// CreatedAt: DateTime (UTC)
    /// UpdatedAt: DateTime (UTC)
    /// </summary>
    public class Supplier
    {

        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public Address? Address { get; set; } = null;
        public string PaymentTerms { get; set; } = string.Empty;
        public int LeadTimeDays { get; set; } = 0;
        public decimal? MinimumOrderValue { get; set; }
        public decimal Rating { get; set; } = 3.0m;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public bool CanFulfillOrder(decimal orderValue)
        {
            if (MinimumOrderValue.HasValue)
            {
                return orderValue >= MinimumOrderValue.Value;
            }
            return true;
        }
    }
}
