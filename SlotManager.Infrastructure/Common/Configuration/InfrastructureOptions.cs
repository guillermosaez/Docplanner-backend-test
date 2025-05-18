namespace SlotManager.Infrastructure.Common.Configuration;

public class InfrastructureOptions
{
    public const string SectionName = "Infrastructure";
    public required RabbitMqOptions RabbitMq { get; init; }
    public required RedisOptions Redis { get; init; }
    public required SlotServiceOptions SlotService { get; init; }
}