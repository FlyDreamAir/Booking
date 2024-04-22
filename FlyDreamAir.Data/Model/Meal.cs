namespace FlyDreamAir.Data.Model;

public class Meal : AddOn
{
    public required Uri ImageSrc { get; set; }

    public required string DishName { get; set; }

    public required string Description { get; set; }
}
