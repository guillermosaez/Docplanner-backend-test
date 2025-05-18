using SlotManager.Domain.GetAvailability;

namespace SlotManager.Domain.UnitTests.GetAvailability;

public class DayOfTheWeekTests
{
    [Fact]
    public void FirstSlotStart_When_requested_Then_result_is_as_expected()
    {
        //Arrange
        var sut = new DayOfTheWeek
        {
            WorkPeriod = new()
            {
                StartHour = 9,
                EndHour = default,
                LunchStartHour = default,
                LunchEndHour = default
            }
        };
        var date = new DateOnly(2025, 5, 19);

        //Act
        var result = sut.FirstSlotStart(date);

        //Assert
        Assert.Equal(new DateTime(2025, 5, 19, 9, 0, 0, DateTimeKind.Utc), result);
    }
    
    [Fact]
    public void LastSlotEnd_When_requested_Then_result_is_as_expected()
    {
        //Arrange
        var sut = new DayOfTheWeek
        {
            WorkPeriod = new()
            {
                StartHour = default,
                EndHour = 18,
                LunchStartHour = default,
                LunchEndHour = default
            }
        };
        var date = new DateOnly(2025, 5, 19);

        //Act
        var result = sut.LastSlotEnd(date);

        //Assert
        Assert.Equal(new DateTime(2025, 5, 19, 18, 0, 0, DateTimeKind.Utc), result);
    }
    
    [Fact]
    public void LunchStart_When_requested_Then_result_is_as_expected()
    {
        //Arrange
        var sut = new DayOfTheWeek
        {
            WorkPeriod = new()
            {
                StartHour = default,
                EndHour = default,
                LunchStartHour = 14,
                LunchEndHour = default
            }
        };
        var date = new DateOnly(2025, 5, 19);

        //Act
        var result = sut.LunchStart(date);

        //Assert
        Assert.Equal(new DateTime(2025, 5, 19, 14, 0, 0, DateTimeKind.Utc), result);
    }
    
    [Fact]
    public void LunchEnd_When_requested_Then_result_is_as_expected()
    {
        //Arrange
        var sut = new DayOfTheWeek
        {
            WorkPeriod = new()
            {
                StartHour = default,
                EndHour = default,
                LunchStartHour = default,
                LunchEndHour = 15
            }
        };
        var date = new DateOnly(2025, 5, 19);

        //Act
        var result = sut.LunchEnd(date);

        //Assert
        Assert.Equal(new DateTime(2025, 5, 19, 15, 0, 0, DateTimeKind.Utc), result);
    }
}