using System.Text.Json.Serialization;

namespace FlyDreamAir.Data.Model;

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Type))]
[JsonDerivedType(typeof(AddOn), nameof(AddOn))]
[JsonDerivedType(typeof(Luggage), nameof(Luggage))]
[JsonDerivedType(typeof(Meal), nameof(Meal))]
[JsonDerivedType(typeof(Seat), nameof(Seat))]
public class AddOn
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Type { get; set; }

    public required decimal Price { get; set; }
}
