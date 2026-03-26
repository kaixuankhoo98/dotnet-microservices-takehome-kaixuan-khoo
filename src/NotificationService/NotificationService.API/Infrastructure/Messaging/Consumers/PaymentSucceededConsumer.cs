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
            "Payment of {Amount:C} for Order {OrderId} confirmed",
            message.CustomerEmail, 
            message.Amount, 
            message.OrderId);
    }
}