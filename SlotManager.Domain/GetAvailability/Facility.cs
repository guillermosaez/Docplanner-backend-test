namespace SlotManager.Domain.GetAvailability;

public class Facility
{
    public required Guid FacilityId { get; init; }
    public required string Name { get; init; }
    public required string Address { get; init; }
}