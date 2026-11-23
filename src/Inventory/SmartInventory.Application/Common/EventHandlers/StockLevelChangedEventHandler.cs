using MediatR;
using SmartInventory.Application.Common.Integration;
using SmartInventory.Application.Common.IntegrationEvents;
using SmartInventory.Domain.Events;


namespace SmartInventory.Application.Common.EventHandlers
{
    public class StockLevelChangedEventHandler(IEnumerable<IIntegrationEventPublisher> eventPublishers) : INotificationHandler<StockLevelChangedEvent>
    {

        public async Task Handle(StockLevelChangedEvent notification, CancellationToken cancellationToken)
        {
            var changeEvent = new StockLevelChangedIntegrationEvent(
                notification.ProductId,
                notification.WarehouseId,
                notification.OldQuantity,
                notification.NewQuantity,
                notification.ChangeReason,
                notification.OccurredAt
            );

            foreach (var eventPublisher in eventPublishers)
                await eventPublisher.PublishAsync(changeEvent, cancellationToken);
        }
    }
}
