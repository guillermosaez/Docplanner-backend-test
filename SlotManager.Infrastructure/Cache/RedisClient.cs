using System.Text.Json;
using SlotManager.Application.Cache;
using StackExchange.Redis;

namespace SlotManager.Infrastructure.Cache;

public class RedisClient : IRedisClient
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    
    public RedisClient(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<T?> GetAsync<T>(string key, Func<Task<T>> callback, TimeSpan expiration)
    {
        var result = await GetAsync<T?>(key);
        if (result is not null) return result;

        var callbackResult = await callback.Invoke();
        await SetAsync(key, callbackResult, expiration);
        return callbackResult;
    }

    public async Task DeleteAsync(string key)
    {
        var database = _connectionMultiplexer.GetDatabase();
        await database.KeyDeleteAsync(key);
    }

    private async Task<T?> GetAsync<T>(string key)
    {
        var database = _connectionMultiplexer.GetDatabase();
        var stringValue = await database.StringGetAsync(key);
        if (stringValue.IsNull)
        {
            return default;
        }
        
        if (typeof(T?) == typeof(string))
        {
            return (T?)Convert.ChangeType(stringValue, typeof(T?));
        }
        var deserializedResult = JsonSerializer.Deserialize<T>(stringValue!);
        return deserializedResult!;
    }
    
    private async Task SetAsync<T>(string key, T input, TimeSpan expiration)
    {
        var database = _connectionMultiplexer.GetDatabase();
        string? inputStringToStore;
        if (typeof(T?) == typeof(string))
        {
            inputStringToStore = (string?)Convert.ChangeType(input, typeof(T?));
        }
        else
        {
            inputStringToStore = JsonSerializer.Serialize(input);
        }
        await database.StringSetAsync(key, inputStringToStore, expiration);
    }
}