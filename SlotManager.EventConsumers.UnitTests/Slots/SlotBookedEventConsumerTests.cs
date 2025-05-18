using MassTransit;
using Moq;
using SlotManager.Application.Cache;
using SlotManager.Domain.TakeSlot;
using SlotManager.EventConsumers.Slots;

namespace SlotManager.EventConsumers.UnitTests.Slots;

public class SlotBookedEventConsumerTests
{
    private readonly Mock<IRedisClient> _redisClientMock = new();

    private SlotBookedEventConsumer _sut => new(_redisClientMock.Object);

    [Fact]
    public async Task Consume_When_received_Then_key_is_deleted()
    {
        //Arrange
        var slotBookedMessage = new SlotBooked
        {
            Slot = new()
            {
                Start = new DateTime(2025, 5, 19, 9, 0, 0, DateTimeKind.Utc),
                End = new DateTime(2025, 5, 19, 9, 30, 0, DateTimeKind.Utc),
                Patient = default,
                FacilityId = default
            }
        };
        var consumeContext = Mock.Of<ConsumeContext<SlotBooked>>(c => c.Message == slotBookedMessage);
        
        //Act
        await _sut.Consume(consumeContext);
        
        //Assert
        _redisClientMock.Verify(r => r.DeleteAsync("05/19/2025"), Times.Once);
    }
}