using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace SlotManager.IntegrationTests.Common.Extensions;

public static class HttpClientExtensions
{
    public static async Task<T?> GetAsync<T>(this HttpClient httpClient, string uri)
    {
        return await httpClient.GetFromJsonAsync<T>(uri);
    }

    public static async Task PostAsync<TBody>(this HttpClient httpClient, string uri, TBody body)
    {
        var serializedBody = JsonSerializer.Serialize(body);
        var content = new StringContent(serializedBody, Encoding.UTF8, MediaTypeNames.Application.Json);
        var httpResponse = await httpClient.PostAsync(uri, content);
        httpResponse.EnsureSuccessStatusCode();
    }
}