using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SlotManager.Domain.TakeSlot;
using SlotManager.EventConsumers.Slots;
using SlotManager.Infrastructure.Cache;
using Testcontainers.Redis;

namespace SlotManager.IntegrationTests.EventConsumers.Slots;

public class SlotBookedEventConsumerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory;

    public SlotBookedEventConsumerTests(WebApplicationFactory<Program> webApplicationFactory)
    {
        _webApplicationFactory = webApplicationFactory;
    }

    [Fact]
    public async Task Consume_When_received_Then_consumption_is_ok()
    {
        //Arrange
        var application = await GetApplicationAsync();
        var testHarness = application.Services.GetTestHarness();
        await testHarness.Start();
        using var scope = application.Services.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var @event = new SlotBooked
        {
            Slot = new()
            {
                Start = default,
                End = default,
                Patient = default,
                FacilityId = default
            }
        };

        //Act
        await publishEndpoint.Publish(@event);
        
        //Assert
        var consumed = testHarness.Consumed.Select<SlotBooked>().First();
        Assert.Equivalent(@event, consumed.Context.Message);
        var consumedByExpectedConsumer = await testHarness.GetConsumerHarness<SlotBookedEventConsumer>().Consumed.Any<SlotBooked>();
        Assert.True(consumedByExpectedConsumer);
    }
    
    private async Task<WebApplicationFactory<Program>> GetApplicationAsync()
    {
        var redisContainer = new RedisBuilder().Build();
        await redisContainer.StartAsync();

        return _webApplicationFactory
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(testServices =>
            {
                var redisConnectionStringBuilderMock = new Mock<IRedisConnectionStringBuilder>();
                redisConnectionStringBuilderMock.Setup(r => r.Build()).Returns(redisContainer.GetConnectionString());
                testServices.AddSingleton(redisConnectionStringBuilderMock.Object);
                testServices.AddMassTransitTestHarness(massTransitHarness =>
                {
                    massTransitHarness.AddConsumer<SlotBookedEventConsumer>();
                    massTransitHarness.UsingInMemory((context, config) => config.ConfigureEndpoints(context));
                });
            }));
    }
}