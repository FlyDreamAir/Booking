namespace FlyDreamAir.Data.Model;

public class Journey
{
    public required Airport From { get; set; }
    public required Airport To { get; set; }
    public required bool IsTwoWay { get; set; }
    public required decimal BaseCost { get; set; }
    public required TimeSpan EstimatedTime { get; set; }
    public required TimeSpan ReturnEstimatedTime { get; set; }
    public required List<Flight> Flights { get; set; } = [];
    public required List<Flight> ReturnFlights {  get; set; } = [];
}
