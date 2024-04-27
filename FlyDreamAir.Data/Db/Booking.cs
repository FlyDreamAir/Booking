using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FlyDreamAir.Data.Db;

public class Booking
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    [Required]
    public required Customer Customer { get; set; }

    [Required]
    public required string From { get; set; }

    [Required]
    public required string To { get; set; }

    public Guid? CancellationId { get; set; }
}
