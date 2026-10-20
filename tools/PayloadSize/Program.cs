using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using RideShare.Api;

var passengerId = "928f4af3-33a8-4d81-9c08-f255540e4fd1";
var tripId = "58fb68fb-6941-4f2c-9d67-7cb85153ec48";
var timestamp = new DateTimeOffset(2026, 10, 19, 10, 30, 0, TimeSpan.Zero);
var jsonPayload = new
{
    tripId,
    passengerId,
    driverId = "",
    status = "REQUESTED",
    pickup = new { latitude = 49.4229, longitude = 26.9871 },
    dropoff = new { latitude = 49.4384, longitude = 26.9872 },
    fareEstimate = 185.5,
    createdAt = timestamp
};
var json = JsonSerializer.SerializeToUtf8Bytes(jsonPayload,
    new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    });

var proto = new TripReply
{
    TripId = tripId,
    PassengerId = passengerId,
    DriverId = "",
    Status = TripStatus.Requested,
    Pickup = new CreateTripRequest.Types.Location { Latitude = 49.4229, Longitude = 26.9871 },
    Dropoff = new CreateTripRequest.Types.Location { Latitude = 49.4384, Longitude = 26.9872 },
    FareEstimate = 185.5,
    CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(timestamp)
};
var protobuf = proto.ToByteArray();
var savedBytes = json.Length - protobuf.Length;
var savedPercent = (double)savedBytes / json.Length * 100;
Console.WriteLine($"JSON UTF-8: {json.Length} bytes");
Console.WriteLine($"Protobuf: {protobuf.Length} bytes");
Console.WriteLine($"Saved: {savedBytes} bytes ({savedPercent:F1}%)");
Console.WriteLine($"JSON: {System.Text.Encoding.UTF8.GetString(json)}");
