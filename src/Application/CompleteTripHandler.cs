using RideShare.Domain;

namespace RideShare.Application;

public sealed class TripNotFoundException(Guid tripId)
    : Exception($"Trip '{tripId}' was not found.");

public sealed class CompleteTripHandler(ITripRepository trips, ITripNotificationPort notifications)
{
    public async Task<Trip> HandleAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        var trip = await trips.FindAsync(tripId, cancellationToken)
            ?? throw new TripNotFoundException(tripId);

        trip.Complete();
        await trips.SaveAsync(trip, cancellationToken);
        await notifications.StatusChangedAsync(trip, cancellationToken);
        return trip;
    }
}
