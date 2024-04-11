using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FlyDreamAir.Data.Db;

public class Payment
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    [Required]
    public required Booking Booking { get; set; }

    [Required]
    public required decimal Amount { get; set; }

    [Required]
    public required string Type { get; set; }
}
