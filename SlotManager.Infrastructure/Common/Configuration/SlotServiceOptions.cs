namespace SlotManager.Infrastructure.Common.Configuration;

public class SlotServiceOptions
{
    public required string ApiBaseUri { get; init; }
    public required AuthenticationOptions Authentication { get; init; }
}