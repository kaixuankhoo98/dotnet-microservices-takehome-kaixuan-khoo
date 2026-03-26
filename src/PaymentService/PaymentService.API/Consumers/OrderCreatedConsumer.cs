using BuildingBlocks.Messaging.Events;
using MassTransit;
using PaymentService.API.Services;

namespace PaymentService.API.Consumers;
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IPaymentAppService paymentAppService;
    private readonly ILogger<OrderCreatedConsumer> logger;

    public OrderCreatedConsumer(
        IPaymentAppService paymentAppService,
        ILogger<OrderCreatedConsumer> logger)
    {
        this.paymentAppService = paymentAppService;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;

        this.logger.LogInformation(
            "Received OrderCreatedEvent. CorrelationId={CorrelationId}, OrderId={OrderId}, Amount={Amount}",
            message.CorrelationId,
            message.OrderId,
            message.Amount);

        await this.paymentAppService.ProcessOrderCreatedAsync(
            message.OrderId,
            message.Amount,
            message.CustomerEmail,
            message.CorrelationId,
            context.CancellationToken);

        this.logger.LogInformation(
            "Finished processing order for payment. CorrelationId={CorrelationId}, OrderId={OrderId}",
            message.CorrelationId,
            message.OrderId);
    }
}