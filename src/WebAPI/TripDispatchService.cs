using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RideShare.Api;
using RideShare.Application;
using RideShare.Domain;

public sealed class TripDispatchService(
    CreateTripHandler createTrip,
    ITripRepository trips) : TripDispatch.TripDispatchBase
{
    public override async Task<TripReply> CreateTrip(RideShare.Api.CreateTripRequest request, ServerCallContext context)
    {
        try
        {
            var trip = await createTrip.HandleAsync(new CreateTripCommand(
                Guid.Parse(request.PassengerId),
                request.Pickup.Latitude, request.Pickup.Longitude,
                request.Dropoff.Latitude, request.Dropoff.Longitude,
                (decimal)request.FareEstimate), context.CancellationToken);
            return ToReply(trip);
        }
        catch (DomainException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (FormatException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }

    public override async Task<TripReply> GetTrip(GetTripRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.TripId, out var tripId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Trip ID must be a UUID."));

        var trip = await trips.FindAsync(tripId, context.CancellationToken);
        return trip is null
            ? throw new RpcException(new Status(StatusCode.NotFound, "Trip was not found."))
            : ToReply(trip);
    }

    public override async Task WatchTripStatus(WatchTripStatusRequest request,
        IServerStreamWriter<TripStatusUpdate> responseStream, ServerCallContext context)
    {
        if (!Guid.TryParse(request.TripId, out var tripId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Trip ID must be a UUID."));

        var trip = await trips.FindAsync(tripId, context.CancellationToken);
        if (trip is null)
            throw new RpcException(new Status(StatusCode.NotFound, "Trip was not found."));

        await responseStream.WriteAsync(new TripStatusUpdate
        {
            TripId = trip.Id.ToString(),
            Status = ToProtoStatus(trip.Status),
            ChangedAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow)
        });
    }

    private static TripReply ToReply(Trip trip) => new()
    {
        TripId = trip.Id.ToString(),
        PassengerId = trip.PassengerId.ToString(),
        DriverId = trip.DriverId?.ToString() ?? string.Empty,
        Status = ToProtoStatus(trip.Status),
        Pickup = new CreateTripRequest.Types.Location
        {
            Latitude = trip.Pickup.Latitude,
            Longitude = trip.Pickup.Longitude
        },
        Dropoff = new CreateTripRequest.Types.Location
        {
            Latitude = trip.Dropoff.Latitude,
            Longitude = trip.Dropoff.Longitude
        },
        FareEstimate = (double)trip.FareEstimate,
        CreatedAt = Timestamp.FromDateTimeOffset(trip.CreatedAt)
    };

    private static RideShare.Api.TripStatus ToProtoStatus(global::RideShare.Domain.TripStatus status) => status switch
    {
        global::RideShare.Domain.TripStatus.Requested => RideShare.Api.TripStatus.Requested,
        global::RideShare.Domain.TripStatus.Matched => RideShare.Api.TripStatus.Matched,
        global::RideShare.Domain.TripStatus.InProgress => RideShare.Api.TripStatus.InProgress,
        global::RideShare.Domain.TripStatus.Completed => RideShare.Api.TripStatus.Completed,
        global::RideShare.Domain.TripStatus.Cancelled => RideShare.Api.TripStatus.Cancelled,
        _ => RideShare.Api.TripStatus.Unspecified
    };
}
