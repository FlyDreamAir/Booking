using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlyDreamAir.Data.Db;

public class Customer
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    [Required]
    public required string Email { get; set; }

    public string? UserName { get; set; }

    [Required]
    public required string FirstName { get; set; }

    [Required]
    public required string LastName { get; set; }

    [Required]
    public required string PassportId { get; set; }

    [Required]
    public required DateOnly DateOfBirth { get; set; }
}
