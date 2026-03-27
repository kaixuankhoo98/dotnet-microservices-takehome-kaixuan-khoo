using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Messaging.Middleware;
using Microsoft.EntityFrameworkCore;
using OrderService.API.Application.Services;
using OrderService.API.Infrastructure.Data;
using OrderService.API.Infrastructure.Persistence.Repositories;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddServiceHealthChecks(builder.Configuration);

builder.AddSerilogLogging("OrderService");

builder.Services.AddDbContext<OrderDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMessaging<OrderDbContext>(builder.Configuration);

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderAppService, OrderAppService>();

var app = builder.Build();

if (app.Configuration.GetValue("ApplyEfMigrations", false))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<OrderDbContext>().Database.MigrateAsync();
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
