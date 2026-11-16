namespace RideShare.Events;

public interface ITripEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
    Guid TripId { get; }
}

public sealed record TripRequestedEvent(Guid EventId, DateTimeOffset OccurredAt, Guid TripId, Guid PassengerId)
    : ITripEvent;

public sealed record TripCompletedEvent(Guid EventId, DateTimeOffset OccurredAt, Guid TripId, Guid PassengerId)
    : ITripEvent;
