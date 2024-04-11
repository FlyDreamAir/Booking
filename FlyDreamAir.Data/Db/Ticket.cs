using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FlyDreamAir.Data.Db;

public class Ticket
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    [Required]
    public required Booking Booking { get; set; }

    [Required]
    public required Flight Flight { get; set; }

    [Required]
    public required string Type { get; set; }

    [Required]
    public required string HolderFirstName { get; set; }

    [Required]
    public required string HolderLastName { get; set; }

    [Required]
    public required string Seat { get; set; }

    [Required]
    public required DateTime DepartureTime { get; set; }
}
