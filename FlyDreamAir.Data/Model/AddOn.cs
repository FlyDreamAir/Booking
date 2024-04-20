using System.Text.Json.Serialization;

namespace FlyDreamAir.Data.Model;

[JsonDerivedType(typeof(AddOn))]
[JsonDerivedType(typeof(Seat))]
public class AddOn
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Type { get; set; }

    public required decimal Price { get; set; }
}
