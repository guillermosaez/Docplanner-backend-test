using FluentResults;
using SlotManager.Application.Common.Extensions;
using SlotManager.Application.Common.Slots;
using SlotManager.Domain.GetAvailability;

namespace SlotManager.Application.TakeSlot.Validations;

public class TakeSlotRequestValidator : ITakeSlotRequestValidator
{
    private readonly ISlotCachedService _slotCachedService;

    public TakeSlotRequestValidator(ISlotCachedService slotCachedService)
    {
        _slotCachedService = slotCachedService;
    }
    
    public async Task<Result> ValidateAsync(TakeSlotCommand request, CancellationToken cancellationToken)
    {
        var startIsEarlierThanEndResult = ValidateStartIsEarlierThanEnd(request);
        if (startIsEarlierThanEndResult.IsFailed)
        {
            return startIsEarlierThanEndResult;
        }
        var availability = await GetAvailabilityAsync(request, cancellationToken);
        var durationValidationResult = ValidateRequestedSlotIsExpectedDuration(request, availability);
        if (durationValidationResult.IsFailed)
        {
            return durationValidationResult;
        }

        var slotIsTakenValidationResult = ValidateIfSlotIsAvailable(request, availability);
        return slotIsTakenValidationResult;
    }
    
    private static Result ValidateStartIsEarlierThanEnd(TakeSlotCommand request)
    {
        var isStartEarlierThanEnd = request.Start!.Value < request.End!.Value;
        return Result.OkIf(isStartEarlierThanEnd, new StartIsNotEarlierThanEndError(request.Start!.Value, request.End!.Value));
    }

    private async Task<Availability> GetAvailabilityAsync(TakeSlotCommand request, CancellationToken cancellationToken)
    {
        var mondayInWeek = DateOnly.FromDateTime(request.Start!.Value).GetPreviousNearestDayInWeek(DayOfWeek.Monday);
        var availability = await _slotCachedService.GetAvailabilityAsync(mondayInWeek, cancellationToken);
        return availability;
    }

    private static Result ValidateRequestedSlotIsExpectedDuration(TakeSlotCommand request, Availability availability)
    {
        var requestedSlotMinuteDuration = (int)request.End!.Value.Subtract(request.Start!.Value).TotalMinutes;
        var isRequestedSlotDurationAsExpected = requestedSlotMinuteDuration == availability.SlotDurationMinutes;
        return Result.OkIf(isRequestedSlotDurationAsExpected, new RequestedSlotDurationIsInvalidError(requestedSlotMinuteDuration, availability.SlotDurationMinutes));
    }
    
    private static Result ValidateIfSlotIsAvailable(TakeSlotCommand request, Availability availability)
    {
        var slotsInDay = request.Start!.Value.DayOfWeek switch
        {
            DayOfWeek.Monday => availability.Monday,
            DayOfWeek.Tuesday => availability.Tuesday,
            DayOfWeek.Wednesday => availability.Wednesday,
            DayOfWeek.Thursday => availability.Thursday,
            DayOfWeek.Friday => availability.Friday,
            DayOfWeek.Saturday => availability.Saturday,
            DayOfWeek.Sunday => availability.Sunday,
            _ => null
        };
        if (slotsInDay is null)
        {
            return Result.Fail(new SlotUnavailableError());
        }

        var requestedSlotStart = request.Start!.Value;
        var requestedSlotEnd = request.End!.Value;
        var date = DateOnly.FromDateTime(requestedSlotStart);
        var lunchStart = slotsInDay.LunchStart(date);
        var lunchEnd = slotsInDay.LunchEnd(date);
        
        var isSlotInWorkingHours = requestedSlotStart >= slotsInDay.FirstSlotStart(date) && requestedSlotEnd <= slotsInDay.LastSlotEnd(date);
        var isBetweenLunchHours = Overlaps(requestedSlotStart, requestedSlotEnd, lunchStart, lunchEnd);
        var isSlotBusy = slotsInDay.BusySlots?.Exists(busySlot => Overlaps(requestedSlotStart, requestedSlotEnd, busySlot.Start, busySlot.End)) == true;
        var isSlotAvailable = isSlotInWorkingHours && !isBetweenLunchHours && !isSlotBusy;
        return Result.OkIf(isSlotAvailable, new SlotUnavailableError());
    }

    private static bool Overlaps(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
    {
        return start1 < end2 && end1 > start2;
    }
}