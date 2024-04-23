using System.Text.Json.Serialization;

namespace FlyDreamAir.Data.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SeatType
{
    Business,
    Premium,
    Economy
}
