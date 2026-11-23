using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Common.Exceptions
{
    public class EntityNotFoundException: Exception
    {
        public EntityNotFoundException() { }
        
        public EntityNotFoundException(string message) : base(message) { }

        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException) { }

        internal static EntityNotFoundException For<T>(Guid id)
        {
            return new EntityNotFoundException($"Entity of type {typeof(T).Name} with ID {id} was not found.");
        }

        internal static EntityNotFoundException For<T>(string key)
        {
            return new EntityNotFoundException($"Entity of type {typeof(T).Name} with key {key} was not found.");
        }
    }
}
