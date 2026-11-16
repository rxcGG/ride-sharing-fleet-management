using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RideShare.Application;
using RideShare.Domain;
using RideShare.Events;

namespace RideShare.Infrastructure;

public sealed class TripJournal(ILogger<TripJournal> log)
{
    private readonly List<ITripEvent> entries = [];

    public Task RecordAsync(ITripEvent tripEvent, CancellationToken cancellationToken)
    {
        entries.Add(tripEvent);
        log.LogInformation("Журнал: записів {Count}, поїздка {TripId}", entries.Count, tripEvent.TripId);
        return Task.CompletedTask;
    }
}

public sealed class TripEventNotifications(ITripRepository trips, ITripNotificationPort notifications,
    IConfiguration config, ILogger<TripEventNotifications> log)
{
    private readonly List<Guid> preparedNotifications = [];

    public async Task PrepareAsync(ITripEvent tripEvent, CancellationToken cancellationToken)
    {
        if (config.GetValue<bool>("Demo:FailNotifications"))
            throw new InvalidOperationException("Демонстраційний збій сповіщення.");

        if (tripEvent is TripCompletedEvent)
        {
            var trip = await trips.FindAsync(tripEvent.TripId, cancellationToken)
                ?? throw new InvalidOperationException("Поїздку не знайдено.");
            await notifications.StatusChangedAsync(trip, cancellationToken);
        }

        preparedNotifications.Add(tripEvent.EventId);
        log.LogInformation("Сповіщення: записів {Count}, поїздка {TripId}", preparedNotifications.Count, tripEvent.TripId);
    }
}
