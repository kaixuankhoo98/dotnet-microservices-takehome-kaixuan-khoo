using Microsoft.AspNetCore.Builder;
using Serilog;

namespace BuildingBlocks.Messaging.Extensions;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(
        this WebApplicationBuilder builder,
        string serviceName
    )
    {
        builder.Host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", serviceName)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] " +
                                    "[CorrelationId:{CorrelationId}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Seq(
                    context.Configuration["Seq:ServerUrl"]
                    ?? context.Configuration["Serilog:WriteTo:1:Args:serverUrl"]
                    ?? "http://localhost:5341");
        });

        return builder;
    }
}