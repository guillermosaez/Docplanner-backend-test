using FluentResults;

namespace SlotManager.Application.TakeSlot.Validations;

public interface ITakeSlotRequestValidator
{
    Task<Result> ValidateAsync(TakeSlotCommand request, CancellationToken cancellationToken);
}