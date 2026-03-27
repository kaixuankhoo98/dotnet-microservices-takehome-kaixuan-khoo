using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Messaging.Middleware;
using Microsoft.EntityFrameworkCore;
using NotificationService.API.Application.Services;
using NotificationService.API.Infrastructure.Data;
using NotificationService.API.Infrastructure.Messaging.Consumers;
using NotificationService.API.Infrastructure.Persistence.Repositories;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddServiceHealthChecks(builder.Configuration);

builder.AddSerilogLogging("NotificationService");

builder.Services.AddDbContext<NotificationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the consumer of 'PaymentSucceededEvent'
// Outbox not necessary as notifications is only a consumer
builder.Services.AddMessaging<NotificationDbContext>(
    builder.Configuration,
    x => x.AddConsumer<PaymentSucceededConsumer>(),
    enableOutbox: false);

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationAppService, NotificationAppService>();

var app = builder.Build();

if (app.Configuration.GetValue("ApplyEfMigrations", false))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<NotificationDbContext>().Database.MigrateAsync();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (!app.Configuration.GetValue("DisableHttpsRedirection", false))
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();