using Microsoft.EntityFrameworkCore;
using NotificationService.API.Data;
using NotificationService.API.Entities;

namespace NotificationService.API.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext notificationDbContext;

    public NotificationRepository(NotificationDbContext notificationDbContext)
    {
        this.notificationDbContext = notificationDbContext;
    }

    public async Task<Notification> CreateAsync(Notification notification, CancellationToken ct)
    {
        this.notificationDbContext.Set<Notification>().Add(notification);
        await notificationDbContext.SaveChangesAsync(ct);
        return notification;
    }

    public async Task<IReadOnlyList<Notification>> GetAllAsync(CancellationToken ct)
    {
        return await this.notificationDbContext
            .Set<Notification>()
            .AsNoTracking()
            .OrderByDescending(n => n.SentAtUtc)
            .ToListAsync(ct);
    }

    public async Task<Notification?> GetByPaymentIdAsync(Guid paymentId, CancellationToken ct)
    {
        return await this.notificationDbContext
            .Set<Notification>()
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.PaymentId == paymentId, ct);
    }
}