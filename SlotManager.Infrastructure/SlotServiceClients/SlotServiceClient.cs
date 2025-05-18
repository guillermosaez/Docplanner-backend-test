using System.Net.Http.Json;
using SlotManager.Domain.GetAvailability;
using SlotManager.Domain.TakeSlot;

namespace SlotManager.Infrastructure.SlotServiceClients;

public class SlotServiceClient : ISlotServiceClient
{
    private readonly HttpClient _httpClient;

    public SlotServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Availability?> GetAvailabilityAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var uri = $"GetWeeklyAvailability/{date:yyyyMMdd}";
        return await _httpClient.GetFromJsonAsync<Availability?>(uri, cancellationToken);
    }

    public async Task TakeSlotAsync(TakeSlotRequest requestBody, CancellationToken cancellationToken)
    {
        const string uri = "TakeSlot";
        await _httpClient.PostAsJsonAsync(uri, requestBody, cancellationToken);
    }
}