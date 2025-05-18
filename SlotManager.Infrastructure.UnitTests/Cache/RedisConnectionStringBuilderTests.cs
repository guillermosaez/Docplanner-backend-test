using Microsoft.Extensions.Options;
using Moq;
using SlotManager.Infrastructure.Cache;
using SlotManager.Infrastructure.Common.Configuration;

namespace SlotManager.Infrastructure.UnitTests.Cache;

public class RedisConnectionStringBuilderTests
{
    private readonly Mock<IOptions<InfrastructureOptions>> _infrastructureOptionsMock = new();
    
    private RedisConnectionStringBuilder _sut => new(_infrastructureOptionsMock.Object);

    [Fact]
    public void Build_When_requested_Then_result_is_as_expected()
    {
        //Arrange
        var redisOptions = new RedisOptions
        {
            Url = "url",
            Password = "password",
            Port = 99
        };
        var infrastructureOptions = new InfrastructureOptions
        {
            Redis = redisOptions,
            RabbitMq = default,
            SlotService = default
        };
        _infrastructureOptionsMock.Setup(i => i.Value).Returns(infrastructureOptions);
        
        //Act
        var result = _sut.Build();
        
        //Assert
        var expectedConnectionString = $"{redisOptions.Url}:{redisOptions.Port},password={redisOptions.Password}";
        Assert.Equal(expectedConnectionString, result);
    }
}