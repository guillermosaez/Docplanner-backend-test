namespace SlotManager.Infrastructure.SlotServiceClients;

public class SlotServiceAuthenticationHandler : DelegatingHandler
{
    private readonly ISlotServiceAuthenticator _slotServiceAuthenticator;

    public SlotServiceAuthenticationHandler(ISlotServiceAuthenticator slotServiceAuthenticator)
    {
        _slotServiceAuthenticator = slotServiceAuthenticator;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authorizationHeader = _slotServiceAuthenticator.BuildHeader();
        request.Headers.Authorization = authorizationHeader;
        return await base.SendAsync(request, cancellationToken);
    }
}