using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Exceptions
{
    public class ProductNotFoundException: Exception
    {
        public Guid ProductId { get; init; }
        public override string Message => $"Product with ID {ProductId} was not found.";
        public ProductNotFoundException(): base() 
        {
        }
    }
}
