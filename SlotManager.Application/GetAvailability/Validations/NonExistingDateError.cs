using FluentResults;

namespace SlotManager.Application.GetAvailability.Validations;

public class NonExistingDateError : Error
{
    public NonExistingDateError(string date) : base(string.Format(Translations.Translations.SlotManager_Application_GetAvailability_Validations_NonExistingDate, date)) { }
}