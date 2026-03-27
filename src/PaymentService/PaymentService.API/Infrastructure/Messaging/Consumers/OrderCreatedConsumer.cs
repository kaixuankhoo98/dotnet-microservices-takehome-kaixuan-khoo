using BuildingBlocks.Messaging.Events;
using MassTransit;
using PaymentService.API.Application.Services;

namespace PaymentService.API.Infrastructure.Messaging.Consumers;
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

        if (!IsValid(message, out var reason))
        {
            logger.LogInformation(
                "Invalid Event. Reason={Reason}",
                reason);
            return;
        }

        logger.LogInformation(
            "Received OrderCreatedEvent. CorrelationId={CorrelationId}, OrderId={OrderId}, Amount={Amount}",
            message.CorrelationId,
            message.OrderId,
            message.Amount);

        await paymentAppService.ProcessOrderCreatedAsync(
            message.OrderId,
            message.Amount,
            message.CustomerEmail,
            message.CorrelationId,
            context.CancellationToken);

        logger.LogInformation(
            "Finished processing order for payment. CorrelationId={CorrelationId}, OrderId={OrderId}",
            message.CorrelationId,
            message.OrderId);
    }

    private static bool IsValid(OrderCreatedEvent message, out string reason)
    {
        if (message.OrderId == Guid.Empty)
        {
            reason = "OrderId is Empty";
            return false;
        }
        if (message.Amount <= 0)
        {
            reason = "Amount must be greater than 0";
            return false;
        }
        if (string.IsNullOrWhiteSpace(message.CustomerEmail))
        {
            reason = "CustomerEmail is required";
            return false;
        }
        reason = string.Empty;
        return true;
    }
}