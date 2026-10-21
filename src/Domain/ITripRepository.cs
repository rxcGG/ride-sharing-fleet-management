namespace RideShare.Domain;

public interface ITripRepository
{
    Task<Trip?> FindAsync(Guid tripId, CancellationToken cancellationToken = default);
    Task SaveAsync(Trip trip, CancellationToken cancellationToken = default);
}
