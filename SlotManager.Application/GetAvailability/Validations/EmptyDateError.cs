using FluentResults;

namespace SlotManager.Application.GetAvailability.Validations;

public class EmptyDateError : Error
{
    public EmptyDateError() : base(Translations.Translations.SlotManager_Application_GetAvailability_Validations_EmptyDate) { }
}