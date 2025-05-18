namespace SlotManager.Application.GetAvailability;

public readonly record struct GetAvailabilityResponseSlot
{
    public DateTime Start { get; init; }
    public DateTime End { get; init; }
}