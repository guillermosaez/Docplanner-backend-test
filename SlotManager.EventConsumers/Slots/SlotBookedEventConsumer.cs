using System.Globalization;
using MassTransit;
using SlotManager.Application.Cache;
using SlotManager.Domain.TakeSlot;

namespace SlotManager.EventConsumers.Slots;

public class SlotBookedEventConsumer : IConsumer<SlotBooked>
{
    private readonly IRedisClient _redisClient;

    public SlotBookedEventConsumer(IRedisClient redisClient)
    {
        _redisClient = redisClient;
    }
    
    public async Task Consume(ConsumeContext<SlotBooked> context)
    {
        var mondayInWeek = DateOnly.FromDateTime(context.Message.Slot.Start);
        var cacheKey = mondayInWeek.ToString(CultureInfo.InvariantCulture);
        await _redisClient.DeleteAsync(cacheKey);
    }
}