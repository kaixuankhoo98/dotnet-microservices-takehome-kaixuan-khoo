using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Messaging.Middleware;
using Microsoft.EntityFrameworkCore;
using PaymentService.API.Consumers;
using PaymentService.API.Data;
using PaymentService.API.Repositories;
using PaymentService.API.Services;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.AddSerilogLogging("PaymentService");

builder.Services.AddDbContext<PaymentDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the consumer of 'OrderCreatedEvent'
builder.Services.AddMessaging<PaymentDbContext>(builder.Configuration, x =>
    x.AddConsumer<OrderCreatedConsumer>());

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentAppService, PaymentAppService>();

var app = builder.Build();

if (app.Configuration.GetValue("ApplyEfMigrations", false))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<PaymentDbContext>().Database.MigrateAsync();
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

app.Run();
