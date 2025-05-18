namespace SlotManager.Infrastructure.Common.Configuration;

public class AuthenticationOptions
{
    public required string User { get; init; }
    public required string Password { get; init; }
}