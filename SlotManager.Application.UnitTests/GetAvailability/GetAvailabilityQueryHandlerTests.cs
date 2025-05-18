using Moq;
using SlotManager.Application.Common.Slots;
using SlotManager.Application.GetAvailability;
using SlotManager.Domain.GetAvailability;

namespace SlotManager.Application.UnitTests.GetAvailability;

public class GetAvailabilityQueryHandlerTests
{
    private readonly Mock<ISlotCachedService> _slotServiceMock = new();

    private GetAvailabilityQueryHandler _sut => new(_slotServiceMock.Object);

    [Fact]
    public async Task Handle_When_validation_fails_Then_error_result_is_returned()
    {
        //Arrange
        var request = new GetAvailabilityQuery { Date = "InvalidDate" };
        
        //Act
        var result = await _sut.Handle(request, default);
        
        //Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task Handle_When_availability_is_requested_Then_response_is_as_expected()
    {
        //Arrange
        var request = new GetAvailabilityQuery { Date = "20250518" };
        var cancellationToken = new CancellationToken(true);
        var slotServiceResponse = new Availability
        {
            Facility = new()
            {
                FacilityId = Guid.NewGuid(),
                Name = "FacilityName",
                Address = "FacilityAddress"
            },
            SlotDurationMinutes = 30,
            Monday = new()
            {
                WorkPeriod = new() { StartHour = 9, LunchStartHour = 14, LunchEndHour = 15, EndHour = 18 },
                BusySlots = null
            },
            Tuesday = new()
            {
                WorkPeriod = new() { StartHour = 11, LunchStartHour = 14, LunchEndHour = 15, EndHour = 17 },
                BusySlots = 
                [
                    new() { Start = new DateTime(2025, 5, 13, 11, 30, 0, DateTimeKind.Utc), End = new DateTime(2025, 5, 13, 12, 0, 0, DateTimeKind.Utc) }, 
                    new() { Start = new DateTime(2025, 5, 13, 15, 30, 0, DateTimeKind.Utc), End = new DateTime(2025, 5, 13, 16, 0, 0, DateTimeKind.Utc) } 
                ]
            },
            Friday = new()
            {
                WorkPeriod = new() { StartHour = 0, LunchStartHour = 0, LunchEndHour = 0, EndHour = 0 }
            }
        };
        _slotServiceMock.Setup(s => s.GetAvailabilityAsync(new DateOnly(2025, 5, 12), cancellationToken)).ReturnsAsync(slotServiceResponse);
    
        //Act
        var result = await _sut.Handle(request, cancellationToken);
    
        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(slotServiceResponse.Facility.FacilityId, result.Value.FacilityId);
        Assert.Equal(3, result.Value.Days.Count);
        AssertMondayResult(result.Value.Days[0], slotServiceResponse.SlotDurationMinutes);
        AssertTuesdayResult(result.Value.Days[1], slotServiceResponse.SlotDurationMinutes);
        AssertFridayResult(result.Value.Days[2]);
    }

    
    private static void AssertMondayResult(GetAvailabilityResponseDay resultMonday, int slotDurationMinutes)
    {
        Assert.Equal(0, resultMonday.DayOfWeek);
        Assert.Equal(16, resultMonday.AvailableSlots.Count);
        
        var startOfTheDay = new DateTime(2025, 5, 12, 9, 0, 0, DateTimeKind.Utc);
        var minutesToAdd = 0;
        
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[0].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[0].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[1].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[1].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[2].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[2].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[3].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[3].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[4].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[4].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[5].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[5].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[6].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[6].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[7].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[7].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[8].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[8].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[9].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[9].End);
        minutesToAdd += 60; //Lunch hour duration
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[10].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[10].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[11].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[11].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[12].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[12].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[13].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[13].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[14].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[14].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[15].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultMonday.AvailableSlots[15].End);
    }
    
    private static void AssertTuesdayResult(GetAvailabilityResponseDay resultTuesday, int slotDurationMinutes)
    {
        Assert.Equal(1, resultTuesday.DayOfWeek);
        Assert.Equal(8, resultTuesday.AvailableSlots.Count);
        
        var startOfTheDay = new DateTime(2025, 5, 13, 11, 0, 0, DateTimeKind.Utc);
        var minutesToAdd = 0;
        
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultTuesday.AvailableSlots[0].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultTuesday.AvailableSlots[0].End);
        minutesToAdd += slotDurationMinutes; //First busy slot
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultTuesday.AvailableSlots[1].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultTuesday.AvailableSlots[1].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultTuesday.AvailableSlots[2].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultTuesday.AvailableSlots[2].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultTuesday.AvailableSlots[3].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultTuesday.AvailableSlots[3].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultTuesday.AvailableSlots[4].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultTuesday.AvailableSlots[4].End);
        minutesToAdd += 60; //Lunch hour duration
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultTuesday.AvailableSlots[5].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultTuesday.AvailableSlots[5].End);
        minutesToAdd += slotDurationMinutes; //Second busy slot
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultTuesday.AvailableSlots[6].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultTuesday.AvailableSlots[6].End);
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultTuesday.AvailableSlots[7].Start);
        minutesToAdd += slotDurationMinutes;
        Assert.Equal(startOfTheDay.AddMinutes(minutesToAdd), resultTuesday.AvailableSlots[7].End);
    }
    
    private static void AssertFridayResult(GetAvailabilityResponseDay resultFriday)
    {
        Assert.Equal(4, resultFriday.DayOfWeek);
        Assert.Empty(resultFriday.AvailableSlots);
    }
}