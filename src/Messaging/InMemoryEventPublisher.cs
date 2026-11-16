using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RideShare.Application;
using RideShare.Events;

namespace RideShare.Messaging;

public sealed class InMemoryEventPublisher(
    IEventBus bus, IConfiguration config, ILogger<InMemoryEventPublisher> log) : IEventPublisher
{
    public void Publish(ITripEvent tripEvent)
    {
        bus.Publish(tripEvent);
        if (config.GetValue<bool>("Demo:RepeatEvents"))
        {
            log.LogInformation("Повторна доставка {EventId}", tripEvent.EventId);
            bus.Publish(tripEvent);
        }
    }
}
