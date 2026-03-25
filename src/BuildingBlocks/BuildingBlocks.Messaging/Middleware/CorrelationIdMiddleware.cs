using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Messaging.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId;
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var headerValue)
            || string.IsNullOrWhiteSpace(headerValue))
        {
            correlationId = Guid.NewGuid().ToString();
        }
        else
        {
            correlationId = headerValue.ToString();
        }

        context.Items["CorrelationId"] = correlationId;

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId;
                return Task.CompletedTask;
            });

            await this.next(context);
        }
    }
}
