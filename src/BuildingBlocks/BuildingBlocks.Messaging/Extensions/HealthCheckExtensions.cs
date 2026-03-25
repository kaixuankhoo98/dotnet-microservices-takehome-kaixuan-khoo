using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace BuildingBlocks.Messaging.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddServiceHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddHealthChecks()
            .AddSqlServer(
                configuration.GetConnectionString("DefaultConnection")!,
                name: "sqlserver",
                tags: ["ready"]
            )
            .AddRabbitMQ(
                sp =>
                {
                    var rabbitHost = configuration["RabbitMq:Host"] ?? "localhost";
                    var rabbitUsername = configuration["RabbitMq:Username"] ?? "guest";
                    var rabbitPassword = configuration["RabbitMq:Password"] ?? "guest";

                    var uri = new Uri($"amqp://{rabbitUsername}:{rabbitPassword}@{rabbitHost}/");
                    var factory = new ConnectionFactory { Uri = uri };
                    
                    // Healthcheck factory is synchronous, client is async only: we block until resolved
                    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
                },
                name: "rabbitmq",
                tags: ["ready"]
            );

        return services;
    }
}