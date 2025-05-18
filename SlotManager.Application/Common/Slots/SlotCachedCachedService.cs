using System.Globalization;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using SlotManager.Application.Cache;
using SlotManager.Application.GetAvailability;
using SlotManager.Domain.GetAvailability;

namespace SlotManager.Application.Common.Slots;

public class SlotCachedCachedService : ISlotCachedService
{
    private readonly ISlotServiceClient _slotServiceClient;
    private readonly IRedisClient _redisClient;
    private readonly ILogger<SlotCachedCachedService> _logger;

    public SlotCachedCachedService(ISlotServiceClient slotServiceClient, IRedisClient redisClient, ILogger<SlotCachedCachedService> logger)
    {
        _slotServiceClient = slotServiceClient;
        _redisClient = redisClient;
        _logger = logger;
    }
    
    public async Task<Availability> GetAvailabilityAsync(DateOnly mondayInWeek, CancellationToken cancellationToken)
    {
        var availability = await GetRetryPolicy().ExecuteAsync(() => DoGetAvailabilityFromServiceAsync(mondayInWeek, cancellationToken));
        if (availability is not null) return availability;
        throw new ExternalSlotServiceUnavailableException();
    }

    private async Task<Availability?> DoGetAvailabilityFromServiceAsync(DateOnly firstDayOfWeek, CancellationToken cancellationToken)
    {
        var cacheKey = firstDayOfWeek.ToString(CultureInfo.InvariantCulture);
        var callback = () => _slotServiceClient.GetAvailabilityAsync(firstDayOfWeek, cancellationToken);
        var expiration = TimeSpan.FromHours(3);

        var response = await _redisClient.GetAsync(cacheKey, callback, expiration);
        return response;
    }

    private AsyncRetryPolicy<Availability?> GetRetryPolicy()
    {
        const int maximumRetries = 2;
        return Policy<Availability?>
            .Handle<Exception>()
            .OrResult(result => result is null)
            .WaitAndRetryAsync(
                retryCount: maximumRetries,
                sleepDurationProvider: retryCount => TimeSpan.FromSeconds(retryCount * 1),
                onRetry: (outcome, _, attemptNumber, _) =>
                {
                    _logger.LogWarning("Error getting WeeklyAvailability from SlotService. Retry {AttemptNumber}/{MaximumRetries}. Reason: {ExceptionMessage}", attemptNumber, maximumRetries, outcome.Exception?.Message);
                }
            );
    }
}