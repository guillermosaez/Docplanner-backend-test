using FluentResults;

namespace SlotManager.Application.TakeSlot.Validations;

public class RequestedSlotDurationIsInvalidError : Error
{
    public RequestedSlotDurationIsInvalidError(int requestedSlotDuration, int expectedDuration) : base(string.Format("Requested slot duration {0} is incorrect. It must be {1}.", requestedSlotDuration, expectedDuration)) { }
}