using SmartInventory.Domain.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Entities
{
    public abstract class BaseEntity
    {
        protected readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

        public void ClearDomainEvents() => _domainEvents.Clear();

    }
}
