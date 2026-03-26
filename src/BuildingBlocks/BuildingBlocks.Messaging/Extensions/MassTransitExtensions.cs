using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddMessaging<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null,
        bool enableOutbox = true
    )
        where TDbContext : DbContext
    {
        services.AddMassTransit(x =>
        {
            if (configureConsumers != null)
            {
                configureConsumers(x);
            }

            // Add outbox for atomic db saving and publishing to queue
            if (enableOutbox)
            {
                x.AddEntityFrameworkOutbox<TDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                    o.QueryDelay = TimeSpan.FromSeconds(1);
                });
            }

            // Rabbit MQ setup
            x.UsingRabbitMq((context, config) =>
            {
                var rabbitHost = configuration["RabbitMq:Host"] ?? "localhost";
                var rabbitUsername = configuration["RabbitMq:Username"] ?? "guest";
                var rabbitPassword = configuration["RabbitMq:Password"] ?? "guest";

                config.Host(rabbitHost, "/", h =>
                {
                    h.Username(rabbitUsername);
                    h.Password(rabbitPassword);
                });

                // Configure retry intervals
                config.UseMessageRetry(r => r.Intervals(
                    TimeSpan.FromMilliseconds(500),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5)));

                config.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
