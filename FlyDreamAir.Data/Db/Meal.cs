using System.ComponentModel.DataAnnotations;

namespace FlyDreamAir.Data.Db;

public class Meal : AddOn
{
    [Required]
    public required Uri ImageSrc { get; set; }

    [Required]
    public required string DishName { get; set; }

    [Required]
    public required string Description { get; set; }
}
