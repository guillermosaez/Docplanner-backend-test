using System.Globalization;
using Microsoft.Extensions.Logging;
using Moq;
using SlotManager.Application.Cache;
using SlotManager.Application.Common.Extensions;
using SlotManager.Application.Common.Slots;
using SlotManager.Application.GetAvailability;
using SlotManager.Domain.GetAvailability;

namespace SlotManager.Application.UnitTests.Common.Slots;

public class SlotCachedCachedServiceTests
{
    private readonly Mock<ISlotServiceClient> _slotServiceClientMock = new();
    private readonly Mock<ILogger<SlotCachedCachedService>> _loggerMock = new();
    private readonly Mock<IRedisClient> _redisClient = new();

    private SlotCachedCachedService _sut => new(_slotServiceClientMock.Object, _redisClient.Object, _loggerMock.Object);

    [Fact]
    public async Task GetAvailabilityAsync_When_client_works_the_first_time_Then_response_is_cached_and_returned()
    {
        //Arrange
        var date = new DateOnly(2025, 5, 18);
        var mondayInWeek = date.GetPreviousNearestDayInWeek(DayOfWeek.Monday);
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

        _redisClient
            .Setup(r => r.GetAsync(
                new DateOnly(2025, 5, 12).ToString(CultureInfo.InvariantCulture),
                It.Is<Func<Task<Availability?>>>(callback => ValidateCallback(callback, slotServiceResponse)),
                TimeSpan.FromHours(3)
            ))
            .ReturnsAsync(slotServiceResponse);
        
        _slotServiceClientMock.Setup(s => s.GetAvailabilityAsync(It.Is<DateOnly>(dt => dt.Day == 12
                                                                                       && dt.Month == 5
                                                                                       && dt.Year == 2025
                                                                                       && dt.DayOfWeek == DayOfWeek.Monday), cancellationToken))
            .ReturnsAsync(slotServiceResponse);
        
        //Act
        var result = await _sut.GetAvailabilityAsync(mondayInWeek, cancellationToken);
        
        //Assert
        Assert.Equal(slotServiceResponse, result);
    }
    
    private static bool ValidateCallback(Func<Task<Availability?>> callback, Availability expected)
    {
        try
        {
            var result = callback().GetAwaiter().GetResult()!;
            return result.SlotDurationMinutes == expected.SlotDurationMinutes
                   && result.Facility == expected.Facility
                   && result.Monday == expected.Monday
                   && result.Tuesday == expected.Tuesday
                   && result.Friday == expected.Friday;
        }
        catch
        {
            return false;
        }
    }
    
    [Fact]
    public async Task Handle_When_service_always_fails_Then_it_is_tried_3_times()
    {
        //Arrange
        var date = new DateOnly(2025, 5, 18);
        var mondayInWeek = date.GetPreviousNearestDayInWeek(DayOfWeek.Monday);
        var cancellationToken = new CancellationToken(true);
        _redisClient
            .Setup(r => r.GetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<Availability?>>>(),
                It.IsAny<TimeSpan>()
            ))
            .ThrowsAsync(new Exception());
        _slotServiceClientMock.Setup(s => s.GetAvailabilityAsync(It.IsAny<DateOnly>(), cancellationToken)).ThrowsAsync(new Exception());
    
        //Act
        var action = () => _sut.GetAvailabilityAsync(mondayInWeek, cancellationToken);
    
        //Assert
        await Assert.ThrowsAsync<Exception>(action);
        _redisClient.Verify(
            r => r.GetAsync(
                new DateOnly(2025, 5, 12).ToString(CultureInfo.InvariantCulture),
                It.IsAny<Func<Task<Availability?>>>(),
                TimeSpan.FromHours(3)
            ),
            Times.Exactly(3)
        );
    }
    
    [Fact]
    public async Task Handle_When_service_fails_two_times_Then_it_is_successful_the_third_time()
    {
        //Arrange
        var date = new DateOnly(2025, 5, 18);
        var mondayInWeek = date.GetPreviousNearestDayInWeek(DayOfWeek.Monday);
        var cancellationToken = new CancellationToken(true);
        var response = new Availability
        {
            Facility = new()
            {
                FacilityId = Guid.NewGuid(),
                Name = string.Empty,
                Address = string.Empty
            },
            SlotDurationMinutes = 33
        };
        _redisClient
            .SetupSequence(r => r.GetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<Availability?>>>(),
                It.IsAny<TimeSpan>()
            ))
            .ThrowsAsync(new Exception())
            .ThrowsAsync(new Exception())
            .ReturnsAsync(response);
    
        //Act
        var result = await _sut.GetAvailabilityAsync(mondayInWeek, cancellationToken);
    
        //Assert
        Assert.Equal(response, result);
        _redisClient.Verify(
            r => r.GetAsync(
                new DateOnly(2025, 5, 12).ToString(CultureInfo.InvariantCulture),
                It.IsAny<Func<Task<Availability?>>>(),
                TimeSpan.FromHours(3)
            ),
            Times.Exactly(3)
        );
    }
    
    [Fact]
    public async Task Handle_When_service_returns_null_three_times_Then_it_is_tried_3_times()
    {
        //Arrange
        var date = new DateOnly(2025, 5, 18);
        var mondayInWeek = date.GetPreviousNearestDayInWeek(DayOfWeek.Monday);
        var cancellationToken = new CancellationToken(true);
    
        //Act
        var action = () => _sut.GetAvailabilityAsync(mondayInWeek, cancellationToken);
    
        //Assert
        await Assert.ThrowsAsync<ExternalSlotServiceUnavailableException>(action);
        _redisClient.Verify(
            r => r.GetAsync(
                new DateOnly(2025, 5, 12).ToString(CultureInfo.InvariantCulture),
                It.IsAny<Func<Task<Availability?>>>(),
                TimeSpan.FromHours(3)
            ),
            Times.Exactly(3)
        );
    }
}