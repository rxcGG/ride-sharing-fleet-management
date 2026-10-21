using Microsoft.AspNetCore.Http.HttpResults;
using RideShare.Application;
using RideShare.Domain;
using RideShare.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Singleton repository keeps the in-memory demo data available between requests.
builder.Services.AddSingleton<ITripRepository, InMemoryTripRepository>();
// Singleton notification adapter holds no request-specific state.
builder.Services.AddSingleton<ITripNotificationPort, ConsoleTripNotificationPort>();
// Use cases are stateless, so create them for each resolution.
builder.Services.AddTransient<CreateTripHandler>();
builder.Services.AddTransient<CompleteTripHandler>();
builder.Services.AddGrpc();

var app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapPost("/trips", async (CreateTripHttpRequest request, CreateTripHandler handler, CancellationToken ct) =>
{
    try
    {
        var trip = await handler.HandleAsync(new CreateTripCommand(
            request.PassengerId, request.Pickup.Latitude, request.Pickup.Longitude,
            request.Dropoff.Latitude, request.Dropoff.Longitude, request.FareEstimate), ct);
        return Results.Created($"/trips/{trip.Id}", TripResponse.From(trip));
    }
    catch (DomainException ex)
    {
        return Results.Problem(type: "about:blank", title: "Invalid trip", detail: ex.Message, statusCode: 422);
    }
});
app.MapPatch("/trips/{tripId:guid}/status", async (Guid tripId, ChangeTripStatusHttpRequest request,
    CompleteTripHandler handler, CancellationToken ct) =>
{
    if (!string.Equals(request.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
        return Results.Problem(type: "about:blank", title: "Unsupported trip status",
            detail: "This demo handler supports the COMPLETED status.", statusCode: 422);

    try
    {
        var trip = await handler.HandleAsync(tripId, ct);
        return Results.Ok(TripResponse.From(trip));
    }
    catch (TripNotFoundException ex)
    {
        return Results.Problem(type: "about:blank", title: "Trip not found", detail: ex.Message, statusCode: 404);
    }
    catch (DomainException ex)
    {
        return Results.Problem(type: "about:blank", title: "Invalid trip status", detail: ex.Message, statusCode: 422);
    }
});
app.MapGrpcService<TripDispatchService>();
app.Run();

public sealed record TripLocationHttp(double Latitude, double Longitude);
public sealed record CreateTripHttpRequest(Guid PassengerId, TripLocationHttp Pickup, TripLocationHttp Dropoff, decimal FareEstimate);
public sealed record ChangeTripStatusHttpRequest(string Status);

public sealed record TripLocationResponse(double Latitude, double Longitude);
public sealed record TripResponse(Guid Id, Guid PassengerId, Guid? DriverId, string Status,
    TripLocationResponse Pickup, TripLocationResponse Dropoff, decimal FareEstimate)
{
    public static TripResponse From(Trip trip) => new(
        trip.Id, trip.PassengerId, trip.DriverId, ToApiStatus(trip.Status),
        new TripLocationResponse(trip.Pickup.Latitude, trip.Pickup.Longitude),
        new TripLocationResponse(trip.Dropoff.Latitude, trip.Dropoff.Longitude), trip.FareEstimate);

    private static string ToApiStatus(TripStatus status) => status switch
    {
        TripStatus.Requested => "REQUESTED",
        TripStatus.Matched => "MATCHED",
        TripStatus.InProgress => "IN_PROGRESS",
        TripStatus.Completed => "COMPLETED",
        TripStatus.Cancelled => "CANCELLED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };
}
