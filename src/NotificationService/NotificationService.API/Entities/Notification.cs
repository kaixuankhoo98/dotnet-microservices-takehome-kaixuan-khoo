using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace NotificationService.API.Entities;
public class Notification
{
    public Guid Id { get; private set; }

    public Guid OrderId {  get; private set; }

    public Guid PaymentId { get; private set; }

    [Precision(18,2)]
    public decimal Amount { get; private set; }

    [MaxLength(256)]
    public string CustomerEmail { get; private set; } = string.Empty;

    [MaxLength(1024)]
    public string Message { get; private set; } = String.Empty;

    public DateTime SentAtUtc { get; private set; }

    private Notification() { }

    public static Notification Create(Guid orderId, Guid paymentId, decimal amount, string customerEmail)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            PaymentId = paymentId,
            Amount = amount,
            CustomerEmail = customerEmail,
            Message = $"Payment of {amount:C} for order {orderId} confirmed.",
            SentAtUtc = DateTime.UtcNow
        };
    }
}
