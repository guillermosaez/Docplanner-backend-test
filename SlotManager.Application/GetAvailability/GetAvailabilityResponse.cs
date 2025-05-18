namespace SlotManager.Application.GetAvailability;

public readonly record struct GetAvailabilityResponse
{
    public Guid FacilityId { get; init; }
    public List<GetAvailabilityResponseDay> Days { get; init; }
}