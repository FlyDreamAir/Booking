namespace FlyDreamAir.Data.Model;

public class PreOrderedAddOn
{
    public required AddOn AddOn { get; set; }
    public required Flight Flight { get; set; }
    public decimal Amount { get; set; }
}
