namespace SlotManager.Application.GetAvailability;

public readonly record struct GetAvailabilityResponseDay
{
    public int DayOfWeek { get; init; }
    public List<GetAvailabilityResponseSlot> AvailableSlots { get; init; }
}