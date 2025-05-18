using SlotManager.Application.Common.Extensions;

namespace SlotManager.Application.UnitTests.Common.Extensions;

public class DateOnlyExtensionsTests
{
    [Theory]
    [MemberData(nameof(InputAndExpectedDates))]
    public void GetPreviousNearestDayInWeek_When_called_Then_result_is_as_expected(DayOfWeek targetDay, DateOnly certainDate, DateOnly expectedStartOfWeek)
    {
        //Act
        var result = certainDate.GetPreviousNearestDayInWeek(targetDay);
        
        //Assert
        Assert.Equal(expectedStartOfWeek, result);
    }


    public static TheoryData<DayOfWeek, DateOnly, DateOnly> InputAndExpectedDates = new()
    {
        {
            DayOfWeek.Monday,
            DateOnly.FromDateTime(new DateTime(2025, 05, 18, 0, 0, 0, DateTimeKind.Utc)),
            DateOnly.FromDateTime(new DateTime(2025, 05, 12, 0, 0, 0, DateTimeKind.Utc))
        },
        {
            DayOfWeek.Monday,
            DateOnly.FromDateTime(new DateTime(2025, 05, 12, 0, 0, 0, DateTimeKind.Utc)),
            DateOnly.FromDateTime(new DateTime(2025, 05, 12, 0, 0, 0, DateTimeKind.Utc))
        },
        {
            DayOfWeek.Saturday,
            DateOnly.FromDateTime(new DateTime(2024, 02, 29, 0, 0, 0, DateTimeKind.Utc)),
            DateOnly.FromDateTime(new DateTime(2024, 02, 24, 0, 0, 0, DateTimeKind.Utc))
        },
        {
            DayOfWeek.Wednesday,
            DateOnly.FromDateTime(new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc)),
            DateOnly.FromDateTime(new DateTime(2024, 12, 25, 0, 0, 0, DateTimeKind.Utc))
        },
        {
            DayOfWeek.Tuesday,
            DateOnly.FromDateTime(new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)),
            DateOnly.FromDateTime(new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc))
        }
    };
}