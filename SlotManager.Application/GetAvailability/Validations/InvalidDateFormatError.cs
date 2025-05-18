using FluentResults;

namespace SlotManager.Application.GetAvailability.Validations;

public class InvalidDateFormatError : Error
{
    public InvalidDateFormatError(string date) : base(string.Format(Translations.Translations.SlotManager_Application_GetAvailability_Validations_InvalidDateFormat, date)) { }
}