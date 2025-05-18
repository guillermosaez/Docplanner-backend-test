using System.Net;
using System.Net.Http.Json;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Moq.Protected;
using SlotManager.Application.TakeSlot;
using SlotManager.Domain.GetAvailability;
using SlotManager.Domain.TakeSlot;
using SlotManager.Infrastructure.Cache;
using SlotManager.Infrastructure.SlotServiceClients;
using SlotManager.IntegrationTests.Common.Extensions;
using Testcontainers.Redis;

namespace SlotManager.IntegrationTests.API.Controllers.SlotsController;

public class TakeSlotTests : IClassFixture<WebApplicationFactory<Program>>
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
        Monday = new()
        {
            WorkPeriod = new()
            {
                StartHour = 9,
                LunchStartHour = 14,
                LunchEndHour = 15,
                EndHour = 18
            },
            BusySlots = []
        },
        SlotDurationMinutes = 60
    };

    public TakeSlotTests(WebApplicationFactory<Program> webApplicationFactory)
    {
        _webApplicationFactory = webApplicationFactory;
    }

    [Fact]
    public async Task TakeSlot_When_requested_Then_slot_request_is_sent_to_service()
    {
        //Arrange
        var (client, testHarness) = await GetHttpClientAsync();

        var slotStart = new DateTime(2025, 5, 19, 11, 0, 0, 0, DateTimeKind.Utc);
        var slotEnd = slotStart.AddHours(1);
        var requestBody = new TakeSlotCommand
        {
            FacilityId = Guid.NewGuid(),
            Start = slotStart,
            End = slotEnd,
            Comments = "My arm hurts a lot",
            Patient = new()
            {
                Name = "Mario",
                SecondName = "Neta",
                Email = "mario.neta@example.com",
                Phone = "555 44 33 22"
            }
        };
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri == new Uri("https://fakeBaseAddress/TakeSlot")), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });
        
        await testHarness.Start();
        
        //Act
        await client.PostAsync(SlotsControllerApiRoutes.Post.TakeSlot, requestBody);
        
        //Assert
        _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri == new Uri("https://fakeBaseAddress/TakeSlot")), ItExpr.IsAny<CancellationToken>());
        var isEventPublished = await testHarness.Published.Any<SlotBooked>();
        Assert.True(isEventPublished);
    }
    
    private async Task<(HttpClient, ITestHarness)> GetHttpClientAsync()
    {
        _httpMessageHandlerMock.Invocations.Clear();
        _httpMessageHandlerMock.Protected() //Mocking an external dependency in an integration test is not ideal. It would be better to have a Testcontainer with the service contained in it, so we can really test the connection to an external dependency.
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(SlotServiceResponse)
            });
            
        var redisContainer = new RedisBuilder().Build();
        await redisContainer.StartAsync();

        var factory = _webApplicationFactory
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
                testServices.AddMassTransitTestHarness(testHarness =>
                {
                    testHarness.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
                });
            }));

        var client = factory.CreateClient();
        var testHarness = factory.Services.CreateScope().ServiceProvider.GetTestHarness();
        
        return (client, testHarness);
    }
}