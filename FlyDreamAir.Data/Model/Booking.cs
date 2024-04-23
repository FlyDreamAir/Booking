namespace FlyDreamAir.Data.Model;

public class Booking
{
    public required Guid Id { get; set; }

    public required Journey Journey { get; set; }

    public required List<PreOrderedAddOn> AddOns {  get; set; }
}
