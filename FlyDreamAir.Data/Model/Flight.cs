namespace FlyDreamAir.Data.Model;

public class Flight
{
    public required string FlightId { get; set; }

    public required Airport From { get; set; }

    public required Airport To { get; set; }

    public required TimeSpan EstimatedTime { get; set; }

    public required DateTimeOffset DepartureTime { get; set; }

    public required decimal BaseCost { get; set; }
}
