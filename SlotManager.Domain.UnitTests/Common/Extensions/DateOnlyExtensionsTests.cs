using SlotManager.Domain.Common.Extensions;

namespace SlotManager.Domain.UnitTests.Common.Extensions;

public class DateOnlyExtensionsTests
{
    [Theory]
    [MemberData(nameof(AddHoursToDateOnlyCases))]
    public void AddHours_When_called_Then_result_is_as_expected(DateOnly certainDate, int hoursToAdd, DateTime expectedResult)
    {
        //Act
        var result = certainDate.AddHours(hoursToAdd);
        
        //Assert
        Assert.Equal(expectedResult, result);
    }
    
    public static TheoryData<DateOnly, int, DateTime> AddHoursToDateOnlyCases = new()
    {
        {
            new DateOnly(2025, 5, 18),
            1,
            new DateTime(2025, 5, 18, 1, 0, 0, DateTimeKind.Utc)
        },
        {
            new DateOnly(2025, 5, 18),
            24,
            new DateTime(2025, 5, 19, 0, 0, 0, DateTimeKind.Utc)
        },
        {
            new DateOnly(2025, 5, 18),
            25,
            new DateTime(2025, 5, 19, 1, 0, 0, DateTimeKind.Utc)
        },
        {
            new DateOnly(2025, 5, 18),
            -1,
            new DateTime(2025, 5, 17, 23, 0, 0, DateTimeKind.Utc)
        }
    };
}