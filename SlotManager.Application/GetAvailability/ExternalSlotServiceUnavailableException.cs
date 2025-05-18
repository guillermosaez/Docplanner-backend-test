namespace SlotManager.Application.GetAvailability;

public class ExternalSlotServiceUnavailableException : Exception
{
    public ExternalSlotServiceUnavailableException() : base(Translations.Translations.SlotManager_Application_GetAvailability_Validations_ExternalSlotServiceUnavailable) { }
}