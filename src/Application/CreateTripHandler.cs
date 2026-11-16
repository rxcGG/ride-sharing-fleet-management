using RideShare.Domain;
using RideShare.Events;

namespace RideShare.Application;

public sealed record CreateTripCommand(Guid PassengerId, double PickupLatitude, double PickupLongitude,
    double DropoffLatitude, double DropoffLongitude, decimal FareEstimate);

public sealed class CreateTripHandler(ITripRepository trips, IEventPublisher events)
{
    public async Task<Trip> HandleAsync(CreateTripCommand command, CancellationToken cancellationToken = default)
    {
        var trip = Trip.Request(
            command.PassengerId,
            new GeoPoint(command.PickupLatitude, command.PickupLongitude),
            new GeoPoint(command.DropoffLatitude, command.DropoffLongitude),
            command.FareEstimate);

        await trips.SaveAsync(trip, cancellationToken);
        events.Publish(new TripRequestedEvent(Guid.NewGuid(), DateTimeOffset.UtcNow, trip.Id, trip.PassengerId));
        return trip;
    }
}
