using BuildingBlocks.Messaging.Events;
using MassTransit;
using OrderService.API.Dtos;
using OrderService.API.Entities;
using OrderService.API.Repositories;

namespace OrderService.API.Services;
public class OrderAppService : IOrderAppService
{
    private readonly IOrderRepository orderRepository;
    private readonly IPublishEndpoint publishEndpoint;
    private readonly ILogger<OrderAppService> logger;

    public OrderAppService(
        IOrderRepository orderRepository,
        IPublishEndpoint publishEndpoint,
        ILogger<OrderAppService> logger
    )
    {
        this.orderRepository = orderRepository;
        this.publishEndpoint = publishEndpoint;
        this.logger = logger;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto request, Guid correlationId, CancellationToken ct)
    {
        var order = Order.Create(request.Amount, request.CustomerEmail);
        await this.orderRepository.AddAsync(order);

        this.logger.LogInformation(
            "Order {OrderId} created for {CustomerEmail}, Amount: {Amount}",
            order.Id, order.CustomerEmail, order.Amount);

        await this.publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = order.Id,
            Amount = order.Amount,
            CustomerEmail = order.CustomerEmail,
            CreatedAtUtc = order.CreatedAtUtc,
            CorrelationId = correlationId,
        });

        this.logger.LogInformation("OrderCreatedEvent published for Order {OrderId}", order.Id);

        // Persist Order and publish OrderCreatedEvent atomically
        await this.orderRepository.SaveChangesAsync(ct);

        return new OrderResponseDto(
            order.Id,
            order.Amount,
            order.CustomerEmail,
            order.CreatedAtUtc);
    }

    public async Task<IReadOnlyList<OrderResponseDto>> GetAllOrdersAsync(CancellationToken ct)
    {
        var orders = await this.orderRepository.GetAllAsync(ct);

        return orders.Select(o => new OrderResponseDto(
            o.Id,
            o.Amount,
            o.CustomerEmail,
            o.CreatedAtUtc
        )).ToList();
    }
}
