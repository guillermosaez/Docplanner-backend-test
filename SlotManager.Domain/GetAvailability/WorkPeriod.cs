namespace SlotManager.Domain.GetAvailability;

public class WorkPeriod
{
    public int StartHour { get; init; }
    public int LunchStartHour { get; init; }
    public int LunchEndHour { get; init; }
    public int EndHour { get; init; }
}