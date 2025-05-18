namespace SlotManager.Application.Common.Extensions;

public static class DateOnlyExtensions
{
    public static DateOnly GetPreviousNearestDayInWeek(this DateOnly date, DayOfWeek targetDay)
    {
        var daysToSubtract = (7 + (date.DayOfWeek - targetDay)) % 7;
        return date.AddDays(-1 * daysToSubtract);
    }
}