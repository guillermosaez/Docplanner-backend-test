namespace SlotManager.Application.Cache;

public interface IRedisClient
{
    Task<T?> GetAsync<T>(string key, Func<Task<T>> callback, TimeSpan expiration);
    Task DeleteAsync(string key);
}