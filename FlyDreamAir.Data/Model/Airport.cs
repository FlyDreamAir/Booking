namespace FlyDreamAir.Data.Model;

public class Airport
{
    public required string Id { get; init; }
    public required string City { get; init; }
    public required string Country { get; init; }
    public required string Name { get; init; }
    public required string TimeZone { get; init; }
}
