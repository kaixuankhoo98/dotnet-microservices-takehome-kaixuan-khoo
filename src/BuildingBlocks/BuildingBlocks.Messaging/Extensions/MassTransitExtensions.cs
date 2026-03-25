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
        Action<IBusRegistrationConfigurator>? configureConsumers = null
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
            x.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox();
                o.QueryDelay = TimeSpan.FromSeconds(1);
            });

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

                config.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
