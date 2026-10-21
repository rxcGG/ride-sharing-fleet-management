using RideShare.Domain;

namespace RideShare.Application;

public interface ITripNotificationPort
{
    Task StatusChangedAsync(Trip trip, CancellationToken cancellationToken = default);
}
