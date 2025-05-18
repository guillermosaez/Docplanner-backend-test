using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using SlotManager.Infrastructure.Common.Configuration;

namespace SlotManager.Infrastructure.SlotServiceClients;

public class SlotServiceAuthenticator : ISlotServiceAuthenticator
{
    private readonly InfrastructureOptions _options;
    
    public SlotServiceAuthenticator(IOptions<InfrastructureOptions> options)
    {
        _options = options.Value;
    }

    public AuthenticationHeaderValue BuildHeader()
    {
        var user = _options.SlotService.Authentication.User;
        var password = _options.SlotService.Authentication.Password;
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{password}"));
        return new("Basic", token);
    }
}