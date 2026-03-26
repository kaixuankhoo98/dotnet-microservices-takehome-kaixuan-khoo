using BuildingBlocks.Messaging.Events;
using MassTransit.Initializers;
using NotificationService.API.Dtos;
using NotificationService.API.Entities;
using NotificationService.API.Repositories;

namespace NotificationService.API.Services;

public class NotificationAppService : INotificationAppService
{
    private readonly INotificationRepository notificationRepository;
    private readonly ILogger<NotificationAppService> logger;

    public NotificationAppService(
        INotificationRepository notificationRepository, 
        ILogger<NotificationAppService> logger)
    {
        this.notificationRepository = notificationRepository;
        this.logger = logger;
    }

    public async Task<IReadOnlyCollection<NotificationResponseDto>> GetAllNotificationsAsync(CancellationToken ct)
    {
        var notifications = await this.notificationRepository.GetAllAsync(ct);

        return notifications.Select(n => new NotificationResponseDto(
            n.Message,
            n.SentAtUtc
        )).ToList();
    }

    public async Task<NotificationResponseDto> ProcessPaymentSucceededAsync(
        Guid paymentId, 
        Guid orderId, 
        decimal amount, 
        string customerEmail, 
        Guid correlationId, 
        CancellationToken ct)
    {
        var existing = await this.notificationRepository.GetByPaymentIdAsync(paymentId, ct);
        if (existing != null)
        {
            this.logger.LogInformation(
                "Notification already exists for PaymentId {PaymentId}, not generating new Notification",
                existing.PaymentId);
            return new NotificationResponseDto(existing.Message, existing.SentAtUtc);
        }

        var notification = Notification.Create(
            orderId,
            paymentId,
            amount,
            customerEmail);

        var result = await this.notificationRepository.CreateAsync(notification, ct);

        return new NotificationResponseDto(result.Message, result.SentAtUtc);
    }
}