using FluentResults;

namespace SlotManager.Application.TakeSlot.Validations;

public class SlotUnavailableError : Error
{
    public SlotUnavailableError(): base(Translations.Translations.SlotManager_Application_GetAvailability_Validations_SlotUnavailable) { }
}