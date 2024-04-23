using System.ComponentModel.DataAnnotations;

namespace FlyDreamAir.Data.Db;

public class Luggage : AddOn
{
    [Required]
    public required Uri ImageSrc { get; set; }

    [Required]
    public required decimal Amount { get; set; }
}
