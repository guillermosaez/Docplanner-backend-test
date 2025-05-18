using System.Globalization;
using FluentResults;
using MediatR;
using SlotManager.Application.Common.Extensions;
using SlotManager.Application.Common.Slots;
using SlotManager.Domain.GetAvailability;

namespace SlotManager.Application.GetAvailability;

public class GetAvailabilityQueryHandler : IRequestHandler<GetAvailabilityQuery, Result<GetAvailabilityResponse>>
{
    private readonly ISlotCachedService _slotCachedService;

    public GetAvailabilityQueryHandler(ISlotCachedService slotCachedService)
    {
        _slotCachedService = slotCachedService;
    }
    
    public async Task<Result<GetAvailabilityResponse>> Handle(GetAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var validationResult = request.Validate();
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        var mondayInWeek = GetMondayInWeek(request.Date!);
        var availableSlots = await _slotCachedService.GetAvailabilityAsync(mondayInWeek, cancellationToken);
        var result = BuildResponse(availableSlots, mondayInWeek);
        return result;
    }
    
    private static DateOnly GetMondayInWeek(string date)
    {
        return ParseDate(date).GetPreviousNearestDayInWeek(DayOfWeek.Monday);
    }

    private static DateOnly ParseDate(string date)
    {
        return DateOnly.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    private static GetAvailabilityResponse BuildResponse(Availability availability, DateOnly firstDayOfWeek)
    {
        var allWeekDays = new List<DayOfTheWeek?> { availability.Monday, availability.Tuesday, availability.Wednesday, availability.Thursday, availability.Friday, availability.Saturday, availability.Sunday };
        var daysWithSlots = allWeekDays
            .Select((day, index) => BuildDayInResponse(day, index, availability.SlotDurationMinutes, firstDayOfWeek))
            .Where(day => day.HasValue)
            .Select(day => day!.Value)
            .ToList();
        
        return new()
        {
            FacilityId = availability.Facility.FacilityId,
            Days = daysWithSlots
        };
    }

    private static GetAvailabilityResponseDay? BuildDayInResponse(DayOfTheWeek? dayAvailability, int dayIndex, int slotDurationMinutes, DateOnly firstDayOfWeek)
    {
        if (dayAvailability is null) return null;
        
        var date = firstDayOfWeek.AddDays(dayIndex);
        var allSlotsInDay = BuildAllSlotsInDay(date, dayAvailability, slotDurationMinutes);
        var availableSlots = RemoveLunchTimeSlots(date, dayAvailability, allSlotsInDay);
        availableSlots = RemoveBusySlots(dayAvailability, availableSlots);
        
        return new()
        {
            DayOfWeek = dayIndex, 
            AvailableSlots = availableSlots.OrderBy(slot => slot.Start).ToList()
        };
    }

    private static List<GetAvailabilityResponseSlot> BuildAllSlotsInDay(DateOnly date, DayOfTheWeek dayAvailability, int slotDurationMinutes)
    {
        var firstSlotInDay = dayAvailability.FirstSlotStart(date);
        var endOfWorkingDay = dayAvailability.LastSlotEnd(date);
        
        var allSlotsInDay = new List<GetAvailabilityResponseSlot>();
        var slotIterator = firstSlotInDay;
        while (slotIterator < endOfWorkingDay)
        {
            var slot = new GetAvailabilityResponseSlot
            {
                Start = slotIterator,
                End = slotIterator.AddMinutes(slotDurationMinutes)
            };
            allSlotsInDay.Add(slot);
            slotIterator = slot.End;
        }

        return allSlotsInDay;
    }

    private static List<GetAvailabilityResponseSlot> RemoveLunchTimeSlots(DateOnly date, DayOfTheWeek dayAvailability, List<GetAvailabilityResponseSlot> allSlotsInDay)
    {
        var lunchStart = dayAvailability.LunchStart(date);
        var lunchEnd = dayAvailability.LunchEnd(date);
        
        return allSlotsInDay
            .Where(s => s.End <= lunchStart || s.Start >= lunchEnd)
            .ToList();
    }

    private static List<GetAvailabilityResponseSlot> RemoveBusySlots(DayOfTheWeek day, List<GetAvailabilityResponseSlot> availableSlots)
    {
        var dayHasBusySlots = day.BusySlots is { Count: > 0 };
        if (!dayHasBusySlots)
        {
            return availableSlots;
        }

        return availableSlots
                .Where(available => !day.BusySlots!.Exists(busy => available.Start == busy.Start && available.End == busy.End))
                .ToList();
    }
}