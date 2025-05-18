using System.Net.Http.Headers;

namespace SlotManager.Infrastructure.SlotServiceClients;

public interface ISlotServiceAuthenticator
{
    AuthenticationHeaderValue BuildHeader();
}