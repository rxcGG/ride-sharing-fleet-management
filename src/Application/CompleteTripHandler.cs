using RideShare.Domain;
using RideShare.Events;

namespace RideShare.Application;

public sealed class TripNotFoundException(Guid tripId)
    : Exception($"Trip '{tripId}' was not found.");

public sealed class CompleteTripHandler(ITripRepository trips, IEventPublisher events)
{
    public async Task<Trip> HandleAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        var trip = await trips.FindAsync(tripId, cancellationToken)
            ?? throw new TripNotFoundException(tripId);

        trip.Complete();
        await trips.SaveAsync(trip, cancellationToken);
        events.Publish(new TripCompletedEvent(Guid.NewGuid(), DateTimeOffset.UtcNow, trip.Id, trip.PassengerId));
        return trip;
    }
}
