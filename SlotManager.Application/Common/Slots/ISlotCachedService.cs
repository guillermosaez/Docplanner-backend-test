using SlotManager.Domain.GetAvailability;

namespace SlotManager.Application.Common.Slots;

public interface ISlotCachedService
{
    Task<Availability> GetAvailabilityAsync(DateOnly mondayInWeek, CancellationToken cancellationToken);
}