namespace RideShare.Domain;

public sealed class Trip
{
    private Trip(Guid id, Guid passengerId, GeoPoint pickup, GeoPoint dropoff, decimal fareEstimate)
    {
        Id = id;
        PassengerId = passengerId;
        Pickup = pickup;
        Dropoff = dropoff;
        FareEstimate = fareEstimate;
        Status = TripStatus.Requested;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; }
    public Guid PassengerId { get; }
    public Guid? DriverId { get; private set; }
    public GeoPoint Pickup { get; }
    public GeoPoint Dropoff { get; }
    public decimal FareEstimate { get; }
    public TripStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public static Trip Request(Guid passengerId, GeoPoint pickup, GeoPoint dropoff, decimal fareEstimate)
    {
        if (passengerId == Guid.Empty)
            throw new DomainException("Passenger ID is required.");
        if (fareEstimate <= 0)
            throw new DomainException("Fare estimate must be positive.");

        return new Trip(Guid.NewGuid(), passengerId, pickup, dropoff, fareEstimate);
    }

    public void AssignDriver(Guid driverId)
    {
        EnsureStatus(TripStatus.Requested);
        if (driverId == Guid.Empty)
            throw new DomainException("Driver ID is required.");

        DriverId = driverId;
        Status = TripStatus.Matched;
    }

    public void Start()
    {
        EnsureStatus(TripStatus.Matched);
        Status = TripStatus.InProgress;
    }

    public void Complete()
    {
        EnsureStatus(TripStatus.InProgress);
        Status = TripStatus.Completed;
    }

    public void Cancel()
    {
        if (Status is TripStatus.Completed or TripStatus.Cancelled)
            throw new DomainException("This trip can no longer be cancelled.");

        Status = TripStatus.Cancelled;
    }

    private void EnsureStatus(TripStatus expected)
    {
        if (Status != expected)
            throw new DomainException($"Expected trip status {expected}, got {Status}.");
    }
}
