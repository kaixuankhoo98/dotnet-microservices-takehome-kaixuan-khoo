using NotificationService.API.Entities;

namespace NotificationService.API.Repositories;
public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification, CancellationToken ct);
    Task<IReadOnlyList<Notification>> GetAllAsync(CancellationToken ct);
    Task<Notification?> GetByPaymentIdAsync(Guid paymentId, CancellationToken ct);
}
