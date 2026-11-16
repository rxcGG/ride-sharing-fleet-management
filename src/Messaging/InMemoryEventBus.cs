using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RideShare.Events;

namespace RideShare.Messaging;

public interface IEventBus
{
    void Publish(ITripEvent tripEvent);
    void Subscribe(string name, Func<ITripEvent, CancellationToken, Task> handle);
}

public sealed class InMemoryEventBus(ILogger<InMemoryEventBus> log) : BackgroundService, IEventBus
{
    private readonly List<Subscription> subscriptions = [];

    // Підписки реєструються в Program.cs перед запуском сервера.
    public void Subscribe(string name, Func<ITripEvent, CancellationToken, Task> handle) =>
        subscriptions.Add(new Subscription(name, handle));

    public void Publish(ITripEvent tripEvent)
    {
        log.LogInformation("Опубліковано {EventType} {EventId} для {Count} споживачів",
            tripEvent.GetType().Name, tripEvent.EventId, subscriptions.Count);
        foreach (var subscription in subscriptions)
            subscription.Queue.Writer.TryWrite(tripEvent);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.WhenAll(subscriptions.Select(subscription => ConsumeAsync(subscription, stoppingToken)));

    private async Task ConsumeAsync(Subscription subscription, CancellationToken stoppingToken)
    {
        // Одна черга й один обробник на підписку, тому перевірка і запис не перетинаються.
        var processed = new HashSet<Guid>();
        await foreach (var tripEvent in subscription.Queue.Reader.ReadAllAsync(stoppingToken))
        {
            if (processed.Contains(tripEvent.EventId))
            {
                log.LogInformation("{Consumer}: повтор {EventId} пропущено", subscription.Name, tripEvent.EventId);
                continue;
            }

            try
            {
                await subscription.Handle(tripEvent, stoppingToken);
                processed.Add(tripEvent.EventId);
                log.LogInformation("{Consumer}: подію {EventId} оброблено", subscription.Name, tripEvent.EventId);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception error)
            {
                log.LogWarning("{Consumer}: помилка обробки {EventId}: {Reason}",
                    subscription.Name, tripEvent.EventId, error.Message);
            }
        }
    }

    private sealed record Subscription(string Name, Func<ITripEvent, CancellationToken, Task> Handle)
    {
        public Channel<ITripEvent> Queue { get; } = Channel.CreateUnbounded<ITripEvent>(
            new UnboundedChannelOptions { SingleReader = true, AllowSynchronousContinuations = false });
    }
}
