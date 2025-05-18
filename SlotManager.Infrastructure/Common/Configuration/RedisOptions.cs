namespace SlotManager.Infrastructure.Common.Configuration;

public class RedisOptions
{
    public required string Url { get; init; }
    public required int Port { get; init; }
    public required string Password { get; init; }
}