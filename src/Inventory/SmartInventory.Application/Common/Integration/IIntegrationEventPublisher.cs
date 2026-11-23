using SmartInventory.Application.Common.IntegrationEvents;
using SmartInventory.Domain.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Common.Integration
{
    public interface IIntegrationEventPublisher
    {
        Task PublishAsync(IIntegrationEvent domainEvent, CancellationToken cancellationToken = default);
    }
}
