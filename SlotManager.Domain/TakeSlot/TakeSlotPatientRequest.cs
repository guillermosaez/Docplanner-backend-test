namespace SlotManager.Domain.TakeSlot;

public class TakeSlotPatientRequest
{
    public required string Name { get; init; }
    public string? SecondName { get; init; }
    public required string Email { get; init; }
    public required string Phone { get; init; }
}
