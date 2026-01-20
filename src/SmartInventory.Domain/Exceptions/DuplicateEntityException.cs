using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Exceptions
{
    public class DuplicateEntityException: Exception
    {
        public Guid EntityId { get; init; }
        public override string Message => $"Entity with ID {EntityId} already exists.";
        public DuplicateEntityException(): base()
        {
        }
    }
}
