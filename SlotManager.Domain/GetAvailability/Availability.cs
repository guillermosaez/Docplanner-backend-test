namespace SlotManager.Domain.GetAvailability;

public class Availability
{
    public required Facility Facility { get; init; }
    public required int SlotDurationMinutes { get; init; }
    public DayOfTheWeek? Monday { get; init; }
    public DayOfTheWeek? Tuesday { get; init; }
    public DayOfTheWeek? Wednesday { get; init; }
    public DayOfTheWeek? Thursday { get; init; }
    public DayOfTheWeek? Friday { get; init; }
    public DayOfTheWeek? Saturday { get; init; }
    public DayOfTheWeek? Sunday { get; init; }
}