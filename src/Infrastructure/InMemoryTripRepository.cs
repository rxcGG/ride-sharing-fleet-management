using System.Collections.Concurrent;
using RideShare.Domain;

namespace RideShare.Infrastructure;

public sealed class InMemoryTripRepository : ITripRepository
{
    private readonly ConcurrentDictionary<Guid, Trip> _trips = new();

    public Task<Trip?> FindAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        _trips.TryGetValue(tripId, out var trip);
        return Task.FromResult(trip);
    }

    public Task SaveAsync(Trip trip, CancellationToken cancellationToken = default)
    {
        _trips[trip.Id] = trip;
        return Task.CompletedTask;
    }
}
