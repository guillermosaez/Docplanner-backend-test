using System.Net;
using System.Net.Http.Json;
using Moq;
using Moq.Protected;
using SlotManager.Domain.GetAvailability;
using SlotManager.Infrastructure.SlotServiceClients;

namespace SlotManager.Infrastructure.UnitTests.SlotServiceClients;

public class SlotServiceClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    
    [Fact]
    public async Task GetAvailabilityAsync_When_requested_Then_response_is_as_expected()
    {
        //Arrange
        var date = new DateOnly(2025, 5, 18);
        var cancellationToken = new CancellationToken(false);
        var expectedResponse = new Availability
        {
            Facility = new()
            {
                FacilityId = Guid.NewGuid(),
                Name = "FacilityName",
                Address = "FacilityAddress"
            },
            SlotDurationMinutes = 60
        };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            });
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new("https://fakeBaseAddress")
        };
        var sut = new SlotServiceClient(httpClient);
        
        //Act
        var result = await sut.GetAvailabilityAsync(date, cancellationToken);

        //Assert
        Assert.Equivalent(expectedResponse, result);
    }
}