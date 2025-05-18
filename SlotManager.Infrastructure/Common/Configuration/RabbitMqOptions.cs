namespace SlotManager.Infrastructure.Common.Configuration;

public class RabbitMqOptions
{
    public required string Host { get; init; }
    public required string User { get; init; }
    public required string Password { get; init; }
}