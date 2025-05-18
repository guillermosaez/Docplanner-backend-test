using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Moq.Protected;
using SlotManager.Application.GetAvailability;
using SlotManager.Domain.GetAvailability;
using SlotManager.Infrastructure.Cache;
using SlotManager.Infrastructure.SlotServiceClients;
using SlotManager.IntegrationTests.Common.Extensions;
using Testcontainers.Redis;

namespace SlotManager.IntegrationTests.API.Controllers.SlotsController;

public class GetAvailabilityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();

    private static readonly Availability SlotServiceResponse = new()
    {
        Facility = new()
        {
            FacilityId = Guid.NewGuid(),
            Name = string.Empty,
            Address = string.Empty
        },
        SlotDurationMinutes = 33
    };

    public GetAvailabilityTests(WebApplicationFactory<Program> webApplicationFactory)
    {
        _webApplicationFactory = webApplicationFactory;
    }

    [Fact]
    public async Task GetAvailability_When_requested_once_Then_expected_availability_is_returned_from_http_service()
    {
        //Arrange
        const string requestedDate = "20250518";
        var client = await GetHttpClientAsync();
        
        //Act
        var response = await client.GetAsync<GetAvailabilityResponse?>(SlotsControllerApiRoutes.Get.GetAvailability(requestedDate));
        Assert.Equal(SlotServiceResponse.Facility.FacilityId, response!.Value.FacilityId); //Expected behaviour for this use case is being tested in the unit test. Here we're only checking that the endpoint returns an expected response.
        Assert.Single(_httpMessageHandlerMock.Invocations);
    }
    
    [Fact]
    public async Task GetAvailability_When_requested_twice_Then_expected_availability_is_returned_from_redis()
    {
        //Arrange
        const string requestedDate = "20250518";
        var client = await GetHttpClientAsync();
        
        //Act
        _ = await client.GetAsync<GetAvailabilityResponse?>(SlotsControllerApiRoutes.Get.GetAvailability(requestedDate));
        _ = await client.GetAsync<GetAvailabilityResponse?>(SlotsControllerApiRoutes.Get.GetAvailability(requestedDate));
        Assert.Single(_httpMessageHandlerMock.Invocations);
    }

    private async Task<HttpClient> GetHttpClientAsync()
    {
        _httpMessageHandlerMock.Invocations.Clear();
        _httpMessageHandlerMock.Protected() //Mocking an external dependency in an integration test is not ideal. It would be better to have a Testcontainer with the service contained in it, so we can really test the connection to an external dependency.
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(SlotServiceResponse)
            });
            
        var redisContainer = new RedisBuilder().Build();
        await redisContainer.StartAsync();
        
        return _webApplicationFactory
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(testServices =>
            {
                testServices.RemoveAll<ISlotServiceClient>();
                testServices.AddTransient<ISlotServiceClient>(_ => new SlotServiceClient(new HttpClient(_httpMessageHandlerMock.Object)
                {
                    BaseAddress = new("https://fakeBaseAddress")
                }));
                var redisConnectionStringBuilderMock = new Mock<IRedisConnectionStringBuilder>();
                redisConnectionStringBuilderMock.Setup(r => r.Build()).Returns(redisContainer.GetConnectionString());
                testServices.AddSingleton(redisConnectionStringBuilderMock.Object);
            }))
            .CreateClient();
    }
}