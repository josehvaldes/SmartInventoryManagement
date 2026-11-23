using Microsoft.Extensions.Logging;
using SmartInventory.Application.Common.Integration;
using SmartInventory.Application.Common.IntegrationEvents;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Infrastructure.AWS.EventPublisher
{
    public class SNSEventPublisher(ILogger<SNSEventPublisher> logger) : IIntegrationEventPublisher
    {
        public async Task PublishAsync(IIntegrationEvent domainEvent, CancellationToken cancellationToken = default)
        {
            logger.LogWarning("SNSEventPublisher.PublishAsync is not implemented. Event: {Event}", domainEvent.GetType().Name);            
        }
    }
}
