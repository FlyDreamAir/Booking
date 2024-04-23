namespace FlyDreamAir.Data.Model;

public class Seat : AddOn
{
    public required SeatType SeatType { get; set; }

    public required int SeatRow {  get; set; }

    public required char SeatPosition { get; set; }

    public required bool IsAvailable { get; set; }

    public bool IsEmergencyRow { get; set; } = false;
}
