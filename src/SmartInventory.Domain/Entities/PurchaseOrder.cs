using SmartInventory.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Entities
{
    /// <summary>
    /// Represents a purchase order in the inventory system.
    //// 
    //     * Id: Guid(Primary Key)
    //     * OrderNumber: string (unique, auto-generated, e.g., "PO-2026-001234")
    //     * SupplierId: Guid(Foreign Key to Supplier)
    //     * WarehouseId: Guid(Foreign Key to Warehouse  * destination)
    //     * OrderDate: DateTime(UTC)
    //     * ExpectedDeliveryDate: DateTime(UTC)
    //     * ActualDeliveryDate: DateTime? (optional, set when received)
    //     * Status: PurchaseOrderStatus(enum)
    //     * SubTotal: decimal (sum of all items)
    //     * TaxAmount: decimal (default: 0)
    //     * ShippingCost: decimal (default: 0)
    //     * TotalAmount: decimal (computed: SubTotal + TaxAmount + ShippingCost)
    //     * Notes: string (optional, max 1000 chars)
    //     * ApprovedBy: string (optional, username/user ID)
    //     * ApprovedAt: DateTime? (optional)
    //     * CreatedAt: DateTime(UTC)
    //     * CreatedBy: string (username/user ID)
    //     * UpdatedAt: DateTime(UTC)
    //     * UpdatedBy: string (username/user ID)         
    //     */
    /// </summary>
    public class PurchaseOrder
    {

        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid SupplierId { get; set; }
        public Guid WarehouseId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public PurchaseOrderStatus Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount
        {
            get
            {
                return SubTotal + TaxAmount + ShippingCost;
            }
        }
        public string Notes { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;

        /*Collections*/
        //(one-to-many, cascade delete)
        public List<PurchaseOrderItem> Items { get; set; } = []; 

    }
}
