namespace SlotManager.IntegrationTests.API.Controllers.SlotsController;

public static class SlotsControllerApiRoutes
{
    private const string BaseUrl = "slots";
    
    public static class Get
    {
        public static string GetAvailability(string date) => $"{BaseUrl}/availability/{date}";
    }

    public static class Post
    {
        public const string TakeSlot = $"{BaseUrl}/take";
    }
}