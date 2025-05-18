using Microsoft.Extensions.Options;
using SlotManager.Infrastructure.Common.Configuration;

namespace SlotManager.Infrastructure.Cache;

public class RedisConnectionStringBuilder : IRedisConnectionStringBuilder
{
    private readonly RedisOptions _options;
    
    public RedisConnectionStringBuilder(IOptions<InfrastructureOptions> infrastructureOptions)
    {
        _options = infrastructureOptions.Value.Redis;
    }

    public string Build() => $"{_options.Url}:{_options.Port},password={_options.Password}";
}