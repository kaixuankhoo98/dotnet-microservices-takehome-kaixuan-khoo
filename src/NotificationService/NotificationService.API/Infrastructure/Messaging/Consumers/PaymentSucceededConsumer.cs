using BuildingBlocks.Messaging.Events;
using MassTransit;
using NotificationService.API.Application.Services;

namespace NotificationService.API.Infrastructure.Messaging.Consumers;
public class PaymentSucceededConsumer : IConsumer<PaymentSucceededEvent>
{
    private readonly INotificationAppService notificationAppService;
    private readonly ILogger<PaymentSucceededConsumer> logger;

    public PaymentSucceededConsumer(
        INotificationAppService notificationAppService, 
        ILogger<PaymentSucceededConsumer> logger)
    {
        this.notificationAppService = notificationAppService;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
    {
        var message = context.Message;

        if (!IsValid(message, out var reason))
        {
            this.logger.LogInformation(
                "Invalid Event. Reason={Reason}",
                reason);
            return;
        }

        this.logger.LogInformation("Received PaymentSucceededEvent. CorrelationId={CorrelationId}, Order={OrderId}, Payment={PaymentId}",
            message.CorrelationId,
            message.OrderId, 
            message.PaymentId);

        await this.notificationAppService.ProcessPaymentSucceededAsync(
            message.PaymentId,
            message.OrderId,
            message.Amount,
            message.CustomerEmail,
            message.CorrelationId,
            context.CancellationToken);

        this.logger.LogInformation(
            "Notification sent to {CustomerEmail}: " +
            "Payment of {Amount} for Order {OrderId} confirmed",
            message.CustomerEmail, 
            message.Amount, 
            message.OrderId);
    }

    private static bool IsValid(PaymentSucceededEvent message, out string reason)
    {
        if (message.OrderId == Guid.Empty)
        {
            reason = "OrderId is Empty";
            return false;
        }
        if (message.PaymentId == Guid.Empty)
        {
            reason = "PaymentId is Empty";
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