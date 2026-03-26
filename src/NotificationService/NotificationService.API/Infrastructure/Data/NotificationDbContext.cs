using Microsoft.EntityFrameworkCore;
using NotificationService.API.Domain.Entities;

namespace NotificationService.API.Infrastructure.Data;
public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.HasIndex(n => n.PaymentId).IsUnique();
            // To future-proof, orderId does not have to be unique (e.g. other types of notifications?)
        });
    }
}
