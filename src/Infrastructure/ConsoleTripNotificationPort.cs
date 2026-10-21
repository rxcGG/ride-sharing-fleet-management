using RideShare.Application;
using RideShare.Domain;

namespace RideShare.Infrastructure;

public sealed class ConsoleTripNotificationPort : ITripNotificationPort
{
    public Task StatusChangedAsync(Trip trip, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Trip {trip.Id} changed to {trip.Status}.");
        return Task.CompletedTask;
    }
}
