using System.ComponentModel.DataAnnotations;

namespace FlyDreamAir.Data.Db;

public class OrderedAddOn
{
    [Required]
    public required Ticket Ticket { get; set; }

    [Required]
    public required AddOn AddOn { get; set; }

    [Required]
    public decimal Amount { get; set; }
}
