using Moq;
using SlotManager.Application.Common.Slots;
using SlotManager.Application.TakeSlot;
using SlotManager.Application.TakeSlot.Validations;
using SlotManager.Domain.GetAvailability;

namespace SlotManager.Application.UnitTests.TakeSlot.Validations;

public class TakeSlotRequestValidatorTests
{
    private readonly Mock<ISlotCachedService> _slotCachedServiceMock = new();

    private TakeSlotRequestValidator _sut => new(_slotCachedServiceMock.Object);

    [Fact]
    public async Task ValidateAsync_When_start_is_not_earlier_than_end_Then_error_is_returned()
    {
        //Arrange
        var request = new TakeSlotCommand
        {
            Start = new DateTime(2025, 5, 19, 14, 0, 0, DateTimeKind.Utc),
            End = new DateTime(2025, 5, 19, 13, 30, 0, DateTimeKind.Utc),
            Patient = default,
            FacilityId = default
        };

        //Act
        var result = await _sut.ValidateAsync(request, default);

        //Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.IsType<StartIsNotEarlierThanEndError>(result.Errors[0]);
        _slotCachedServiceMock.Verify(s => s.GetAvailabilityAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_When_requested_slot_duration_is_shorter_than_availability_duration_Then_error_is_returned()
    {
        //Arrange
        var request = new TakeSlotCommand
        {
            Start = new DateTime(2025, 5, 19, 10, 0, 0, DateTimeKind.Utc),
            End = new DateTime(2025, 5, 19, 10, 30, 0, DateTimeKind.Utc),
            Patient = default,
            FacilityId = default
        };
        var cancellationToken = new CancellationToken(true);

        var availability = new Availability
        {
            SlotDurationMinutes = 60,
            Facility = new()
            {
                FacilityId = Guid.Empty,
                Name = string.Empty,
                Address = string.Empty
            }
        };

        _slotCachedServiceMock
            .Setup(s => s.GetAvailabilityAsync(new DateOnly(2025, 5, 19), cancellationToken))
            .ReturnsAsync(availability);

        //Act
        var result = await _sut.ValidateAsync(request, cancellationToken);

        //Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.IsType<RequestedSlotDurationIsInvalidError>(result.Errors[0]);
    }

    [Fact]
    public async Task ValidateAsync_When_requested_slot_duration_is_longer_than_availability_duration_Then_error_is_returned()
    {
        //Arrange
        var request = new TakeSlotCommand
        {
            Start = new DateTime(2025, 5, 19, 10, 0, 0, DateTimeKind.Utc),
            End = new DateTime(2025, 5, 19, 11, 30, 0, DateTimeKind.Utc),
            Patient = default,
            FacilityId = default
        };
        var cancellationToken = new CancellationToken(true);

        var availability = new Availability
        {
            SlotDurationMinutes = 60,
            Facility = new()
            {
                FacilityId = Guid.Empty,
                Name = string.Empty,
                Address = string.Empty
            }
        };

        _slotCachedServiceMock
            .Setup(s => s.GetAvailabilityAsync(new DateOnly(2025, 5, 19), cancellationToken))
            .ReturnsAsync(availability);

        //Act
        var result = await _sut.ValidateAsync(request, cancellationToken);

        //Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.IsType<RequestedSlotDurationIsInvalidError>(result.Errors[0]);
    }

    [Theory]
    [MemberData(nameof(RequestSlotsForLunchTest))]
    public async Task ValidateAsync_When_requested_slot_is_during_lunchtime_Then_error_is_returned(DateTime requestStart, DateTime requestEnd)
    {
        //Arrange
        var request = new TakeSlotCommand
        {
            Start = requestStart,
            End = requestEnd,
            Patient = default,
            FacilityId = default
        };
        var cancellationToken = new CancellationToken(true);

        var availability = new Availability
        {
            SlotDurationMinutes = 60,
            Monday = new DayOfTheWeek
            {
                WorkPeriod = new() { StartHour = 9, LunchStartHour = 13, LunchEndHour = 14, EndHour = 17 },
                BusySlots = []
            },
            Facility = new()
            {
                FacilityId = Guid.Empty,
                Name = string.Empty,
                Address = string.Empty
            }
        };

        _slotCachedServiceMock
            .Setup(s => s.GetAvailabilityAsync(new DateOnly(2025, 5, 19), cancellationToken))
            .ReturnsAsync(availability);

        //Act
        var result = await _sut.ValidateAsync(request, cancellationToken);

        //Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.IsType<SlotUnavailableError>(result.Errors[0]);
    }

    public static TheoryData<DateTime, DateTime> RequestSlotsForLunchTest = new()
    {
        {
            new DateTime(2025, 5, 19, 13, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 5, 19, 14, 0, 0, DateTimeKind.Utc)
        },
        {
            new DateTime(2025, 5, 19, 12, 30, 0, DateTimeKind.Utc),
            new DateTime(2025, 5, 19, 13, 30, 0, DateTimeKind.Utc)
        },
        {
            new DateTime(2025, 5, 19, 13, 30, 0, DateTimeKind.Utc),
            new DateTime(2025, 5, 19, 14, 30, 0, DateTimeKind.Utc)
        }
    };

    [Theory]
    [MemberData(nameof(RequestSlotsForBusySlotsTest))]
    public async Task ValidateAsync_When_requested_slot_is_during_busy_slot_Then_error_is_returned(DateTime requestStart, DateTime requestEnd, Slot busySlot)
    {
        //Arrange
        var request = new TakeSlotCommand
        {
            Start = requestStart,
            End = requestEnd,
            Patient = default,
            FacilityId = default
        };
        var cancellationToken = new CancellationToken(true);

        var availability = new Availability
        {
            SlotDurationMinutes = 60,
            Monday = new DayOfTheWeek
            {
                WorkPeriod = new() { StartHour = 9, LunchStartHour = 13, LunchEndHour = 14, EndHour = 17 },
                BusySlots = [busySlot]
            },
            Facility = new()
            {
                FacilityId = Guid.Empty,
                Name = string.Empty,
                Address = string.Empty
            }
        };

        _slotCachedServiceMock
            .Setup(s => s.GetAvailabilityAsync(new DateOnly(2025, 5, 19), cancellationToken))
            .ReturnsAsync(availability);

        //Act
        var result = await _sut.ValidateAsync(request, cancellationToken);

        //Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.IsType<SlotUnavailableError>(result.Errors[0]);
    }

    public static TheoryData<DateTime, DateTime, Slot> RequestSlotsForBusySlotsTest = new()
    {
        {
            new DateTime(2025, 5, 19, 13, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 5, 19, 14, 0, 0, DateTimeKind.Utc),
            new(){ Start = new DateTime(2025, 5, 19, 13, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 5, 19, 14, 0, 0, DateTimeKind.Utc) }
        },
        {
            new DateTime(2025, 5, 19, 12, 30, 0, DateTimeKind.Utc),
            new DateTime(2025, 5, 19, 13, 30, 0, DateTimeKind.Utc),
            new(){ Start = new DateTime(2025, 5, 19, 13, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 5, 19, 14, 0, 0, DateTimeKind.Utc) }
        },
        {
            new DateTime(2025, 5, 19, 13, 30, 0, DateTimeKind.Utc),
            new DateTime(2025, 5, 19, 14, 30, 0, DateTimeKind.Utc),
            new(){ Start = new DateTime(2025, 5, 19, 13, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 5, 19, 14, 0, 0, DateTimeKind.Utc) }
        }
    };

    [Theory]
    [MemberData(nameof(RequestSlotsForOutOfWorkingHoursTest))]
    public async Task ValidateAsync_When_requested_slot_is_out_of_working_hours_Then_error_is_returned(DateTime requestStart, DateTime requestEnd)
    {
        //Arrange
        var request = new TakeSlotCommand
        {
            Start = requestStart,
            End = requestEnd,
            Patient = default,
            FacilityId = default
        };
        var cancellationToken = new CancellationToken(true);

        var availability = new Availability
        {
            SlotDurationMinutes = 60,
            Monday = new DayOfTheWeek
            {
                WorkPeriod = new() { StartHour = 9, LunchStartHour = 14, LunchEndHour = 15, EndHour = 17 },
                BusySlots = []
            },
            Facility = new()
            {
                FacilityId = Guid.Empty,
                Name = string.Empty,
                Address = string.Empty
            }
        };

        _slotCachedServiceMock
            .Setup(s => s.GetAvailabilityAsync(new DateOnly(2025, 5, 19), cancellationToken))
            .ReturnsAsync(availability);

        //Act
        var result = await _sut.ValidateAsync(request, cancellationToken);

        //Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.IsType<SlotUnavailableError>(result.Errors[0]);
    }

    public static TheoryData<DateTime, DateTime> RequestSlotsForOutOfWorkingHoursTest = new()
    {
        {
            new DateTime(2025, 5, 19, 8, 30, 0, DateTimeKind.Utc),
            new DateTime(2025, 5, 19, 9, 30, 0, DateTimeKind.Utc)
        },
        {
            new DateTime(2025, 5, 19, 16, 30, 0, DateTimeKind.Utc),
            new DateTime(2025, 5, 19, 17, 30, 0, DateTimeKind.Utc)
        }
    };

    [Fact]
    public async Task ValidateAsync_When_requested_slot_day_has_no_availability_Then_error_is_returned()
    {
        //Arrange
        var request = new TakeSlotCommand
        {
            Start = new DateTime(2025, 5, 19, 16, 30, 0, DateTimeKind.Utc),
            End = new DateTime(2025, 5, 19, 17, 30, 0, DateTimeKind.Utc),
            Patient = default,
            FacilityId = default
        };
        var cancellationToken = new CancellationToken(true);

        var availability = new Availability
        {
            SlotDurationMinutes = 60,
            Facility = new()
            {
                FacilityId = Guid.Empty,
                Name = string.Empty,
                Address = string.Empty
            }
        };

        _slotCachedServiceMock
            .Setup(s => s.GetAvailabilityAsync(new DateOnly(2025, 5, 19), cancellationToken))
            .ReturnsAsync(availability);

        //Act
        var result = await _sut.ValidateAsync(request, cancellationToken);

        //Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.IsType<SlotUnavailableError>(result.Errors[0]);
    }

    [Fact]
    public async Task ValidateAsync_When_all_is_ok_Then_ok_result_is_returned()
    {
        //Arrange
        var request = new TakeSlotCommand
        {
            Start = new DateTime(2025, 5, 19, 10, 0, 0, DateTimeKind.Utc),
            End = new DateTime(2025, 5, 19, 10, 30, 0, DateTimeKind.Utc),
            Patient = default,
            FacilityId = default
        };
        var cancellationToken = new CancellationToken(true);

        var availability = new Availability
        {
            SlotDurationMinutes = 30,
            Monday = new DayOfTheWeek
            {
                WorkPeriod = new() { StartHour = 9, LunchStartHour = 13, LunchEndHour = 14, EndHour = 17 },
                BusySlots =
                [
                    new(){ Start = new DateTime(2025, 5, 19, 9, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 5, 19, 9, 30, 0, DateTimeKind.Utc) },
                    new(){ Start = new DateTime(2025, 5, 19, 9, 30, 0, DateTimeKind.Utc), End = new DateTime(2025, 5, 19, 10, 0, 0, DateTimeKind.Utc) },
                    new(){ Start = new DateTime(2025, 5, 19, 10, 30, 0, DateTimeKind.Utc), End = new DateTime(2025, 5, 19, 11, 0, 0, DateTimeKind.Utc) }
                ]
            },
            Facility = new()
            {
                FacilityId = Guid.Empty,
                Name = string.Empty,
                Address = string.Empty
            }
        };

        _slotCachedServiceMock
            .Setup(s => s.GetAvailabilityAsync(new DateOnly(2025, 5, 19), cancellationToken))
            .ReturnsAsync(availability);

        //Act
        var result = await _sut.ValidateAsync(request, cancellationToken);

        //Assert
        Assert.True(result.IsSuccess);
    }
}