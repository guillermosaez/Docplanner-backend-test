namespace SlotManager.Domain.TakeSlot;

public class SlotBooked
{
    public required TakeSlotRequest Slot { get; init; }
}