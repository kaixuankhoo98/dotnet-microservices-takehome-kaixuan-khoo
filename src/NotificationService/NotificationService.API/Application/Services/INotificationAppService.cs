using NotificationService.API.Application.Dtos;

namespace NotificationService.API.Application.Services;
public interface INotificationAppService
{
    Task<NotificationResponseDto> ProcessPaymentSucceededAsync(
        Guid paymentId,
        Guid orderId,
        decimal amount, 
        string customerEmail,
        Guid correlationId,
        CancellationToken ct);

    Task<IReadOnlyCollection<NotificationResponseDto>> GetAllNotificationsAsync(CancellationToken ct);
}
