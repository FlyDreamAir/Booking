using System.ComponentModel.DataAnnotations;

namespace FlyDreamAir.Data.Db;

public class CreditCardPayment : Payment
{
    [Required]
    [CreditCard]
    public required string CardNumber { get; set; }

    [Required]
    public required string CardName { get; set; }
}
