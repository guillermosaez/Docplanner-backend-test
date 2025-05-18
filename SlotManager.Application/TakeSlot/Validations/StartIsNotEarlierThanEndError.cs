using System.Globalization;
using FluentResults;

namespace SlotManager.Application.TakeSlot.Validations;

public class StartIsNotEarlierThanEndError : Error
{
    public StartIsNotEarlierThanEndError(DateTime start, DateTime end)
        : base(string.Format(Translations.Translations.SlotManager_Application_GetAvailability_Validations_StartIsNotEarlierThanEnd, start.ToString(CultureInfo.InvariantCulture), end.ToString(CultureInfo.InvariantCulture))) { }
}