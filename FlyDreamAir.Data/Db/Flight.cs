using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlyDreamAir.Data.Db;

public class Flight
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    [Required]
    public required string FlightId { get; set; }

    [Required]
    public required string FromAirport { get; set; }

    [Required]
    public required string ToAirport { get; set; }

    [Required]
    public required TimeSpan EstimatedTime { get; set; }

    [Required]
    public required decimal BaseCost { get; set; }
}
