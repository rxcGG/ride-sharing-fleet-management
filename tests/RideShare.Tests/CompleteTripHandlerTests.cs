using RideShare.Application;
using RideShare.Domain;
using RideShare.Events;

namespace RideShare.Tests;

public sealed class CompleteTripHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenTripIsInProgress_SavesAndPublishes()
    {
        var trip = CreateInProgressTrip();
        var repository = new FakeTripRepository(trip);
        var events = new FakeEventPublisher();
        var handler = new CompleteTripHandler(repository, events);

        var result = await handler.HandleAsync(trip.Id);

        Assert.Equal(TripStatus.Completed, result.Status);
        Assert.Equal(1, repository.SaveCount);
        Assert.Equal(trip.Id, Assert.IsType<TripCompletedEvent>(Assert.Single(events.Published)).TripId);
    }

    [Fact]
    public async Task HandleAsync_WhenTripIsNotInProgress_RejectsWithoutSaving()
    {
        var trip = Trip.Request(Guid.NewGuid(), new GeoPoint(49.42, 26.98), new GeoPoint(49.43, 26.99), 100m);
        var repository = new FakeTripRepository(trip);
        var handler = new CompleteTripHandler(repository, new FakeEventPublisher());

        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(trip.Id));

        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryFails_PropagatesPortError()
    {
        var trip = CreateInProgressTrip();
        var repository = new FakeTripRepository(trip) { SaveError = new IOException("Storage port failed.") };
        var events = new FakeEventPublisher();
        var handler = new CompleteTripHandler(repository, events);

        var error = await Assert.ThrowsAsync<IOException>(() => handler.HandleAsync(trip.Id));

        Assert.Equal("Storage port failed.", error.Message);
        Assert.Empty(events.Published);
    }

    private static Trip CreateInProgressTrip()
    {
        var trip = Trip.Request(Guid.NewGuid(), new GeoPoint(49.42, 26.98), new GeoPoint(49.43, 26.99), 100m);
        trip.AssignDriver(Guid.NewGuid());
        trip.Start();
        return trip;
    }
}

internal sealed class FakeTripRepository(Trip? trip) : ITripRepository
{
    public int SaveCount { get; private set; }
    public Exception? SaveError { get; init; }

    public Task<Trip?> FindAsync(Guid tripId, CancellationToken cancellationToken = default) =>
        Task.FromResult(trip?.Id == tripId ? trip : null);

    public Task SaveAsync(Trip savedTrip, CancellationToken cancellationToken = default)
    {
        if (SaveError is not null)
            throw SaveError;
        SaveCount++;
        return Task.CompletedTask;
    }
}

internal sealed class FakeEventPublisher : IEventPublisher
{
    public List<ITripEvent> Published { get; } = [];

    public void Publish(ITripEvent tripEvent) => Published.Add(tripEvent);
}
