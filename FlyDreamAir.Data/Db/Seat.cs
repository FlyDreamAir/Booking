using System.ComponentModel.DataAnnotations;

namespace FlyDreamAir.Data.Db;

public class Seat : AddOn
{
    [Required]
    public required Flight Flight { get; set; }

    [Required]
    public required SeatType SeatType { get; set; }

    [Required]
    public required int SeatRow {  get; set; }

    [Required]
    public required char SeatPosition { get; set; }

    public bool IsEmergencyRow { get; set; } = false;
}
