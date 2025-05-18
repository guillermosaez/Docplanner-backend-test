using SlotManager.Domain.Common.Extensions;

namespace SlotManager.Domain.GetAvailability;

public class DayOfTheWeek
{
    public WorkPeriod? WorkPeriod { get; init; }
    public List<Slot>? BusySlots { get; init; }

    public DateTime FirstSlotStart(DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(WorkPeriod);
        return date.AddHours(WorkPeriod.StartHour);
    }

    public DateTime LastSlotEnd(DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(WorkPeriod);
        return date.AddHours(WorkPeriod.EndHour);
    }

    public DateTime LunchStart(DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(WorkPeriod);
        return date.AddHours(WorkPeriod.LunchStartHour);
    }

    public DateTime LunchEnd(DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(WorkPeriod);
        return date.AddHours(WorkPeriod.LunchEndHour);
    }
}