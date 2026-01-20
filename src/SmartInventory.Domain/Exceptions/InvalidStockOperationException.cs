using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Exceptions
{
    public class InvalidStockOperationException: Exception
    {
        public string Reason { get; init; } = string.Empty;
        public override string Message => $"Invalid stock operation: {Reason}";
        public InvalidStockOperationException(): base() { }
    }
}
