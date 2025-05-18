using MassTransit;
using Microsoft.Extensions.Options;
using SlotManager.API.Middlewares;
using SlotManager.Application.Cache;
using SlotManager.Application.Common.Slots;
using SlotManager.Application.TakeSlot.Validations;
using SlotManager.Domain.GetAvailability;
using SlotManager.Infrastructure.Cache;
using SlotManager.Infrastructure.Common.Configuration;
using SlotManager.Infrastructure.SlotServiceClients;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<InfrastructureOptions>(builder.Configuration.GetSection(InfrastructureOptions.SectionName));
builder.Services.AddScoped<ISlotCachedService, SlotCachedCachedService>();
builder.Services.AddScoped<ISlotServiceAuthenticator, SlotServiceAuthenticator>();
builder.Services.AddScoped<SlotServiceAuthenticationHandler>();
builder.Services.AddHttpClient<ISlotServiceClient, SlotServiceClient>((serviceProvider, httpClient) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<InfrastructureOptions>>();
    httpClient.BaseAddress = new(options.Value.SlotService.ApiBaseUri);
}).AddHttpMessageHandler<SlotServiceAuthenticationHandler>();

builder.Services.AddSingleton<IRedisConnectionStringBuilder, RedisConnectionStringBuilder>();
builder.Services.AddScoped<IRedisClient, RedisClient>();
builder.Services.AddSingleton<IConnectionMultiplexer>(services =>
{
    var redisConnectionStringBuilder = services.GetRequiredService<IRedisConnectionStringBuilder>();
    return ConnectionMultiplexer.Connect(redisConnectionStringBuilder.Build());
});
builder.Services.AddScoped<ITakeSlotRequestValidator, TakeSlotRequestValidator>();
builder.Services.AddMassTransit(massTransitConfig =>
{
    massTransitConfig.AddConsumers(AppDomain.CurrentDomain.Load("SlotManager.EventConsumers"));
    massTransitConfig.UsingRabbitMq((context, rabbitMqConfig) =>
    {
        var infrastructureOptions = context.GetRequiredService<IOptions<InfrastructureOptions>>().Value;
        rabbitMqConfig.Host(infrastructureOptions.RabbitMq.Host, host =>
        {
            host.Username(infrastructureOptions.RabbitMq.User);
            host.Password(infrastructureOptions.RabbitMq.Password);
        });
        rabbitMqConfig.ConfigureEndpoints(context);
    });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(mediatrConfig => mediatrConfig.RegisterServicesFromAssembly(AppDomain.CurrentDomain.Load("SlotManager.Application")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ExceptionHandler>();

app.MapControllers();

await app.RunAsync();

// Make the implicit Program class public so test projects can access it
public partial class Program { }