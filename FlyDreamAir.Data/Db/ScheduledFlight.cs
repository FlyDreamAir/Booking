using System.ComponentModel.DataAnnotations;

namespace FlyDreamAir.Data.Db;

public class ScheduledFlight
{
    [Required]
    public required Flight Flight { get; set; }

    [Required]
    public required DateTime DepartureTime { get; set; }
}
