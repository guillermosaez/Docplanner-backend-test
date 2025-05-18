namespace SlotManager.Domain.TakeSlot;

public class TakeSlotRequest
{
    public required DateTime Start { get; init; }
    public required DateTime End { get; init; }
    public string? Comments { get; init; }
    public required TakeSlotPatientRequest Patient { get; init; }
    public required Guid FacilityId { get; init; }
}