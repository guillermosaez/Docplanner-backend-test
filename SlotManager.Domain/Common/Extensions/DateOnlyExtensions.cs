namespace SlotManager.Domain.Common.Extensions;

public static class DateOnlyExtensions
{
    public static DateTime AddHours(this DateOnly date, int hours)
    {
        return date.ToDateTime(TimeOnly.MinValue).AddHours(hours);
    }
}