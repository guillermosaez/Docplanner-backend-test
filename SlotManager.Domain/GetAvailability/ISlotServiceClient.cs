using SlotManager.Domain.TakeSlot;

namespace SlotManager.Domain.GetAvailability;

public interface ISlotServiceClient
{
    Task<Availability?> GetAvailabilityAsync(DateOnly date, CancellationToken cancellationToken);
    Task TakeSlotAsync(TakeSlotRequest requestBody, CancellationToken cancellationToken);
}