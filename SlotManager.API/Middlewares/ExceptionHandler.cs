using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace SlotManager.API.Middlewares;

public class ExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<ExceptionHandler>();
    }
    
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = new ProblemDetails
        {
            Type = exception.GetType().ToString(),
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = exception.Message
        };
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = problemDetails.Status.Value;
        var payload = JsonSerializer.Serialize(problemDetails);
        _logger.LogError(exception, "{Origin}, Exception: {Payload}", nameof(ExceptionHandler), payload);
        await context.Response.WriteAsync(payload);
    }
}