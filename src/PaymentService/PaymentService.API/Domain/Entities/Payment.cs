using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PaymentService.API.Domain.Entities;

public class Payment
{
    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    [Precision(18,2)]
    public decimal Amount { get; private set; }

    [MaxLength(256)]
    public string CustomerEmail { get; private set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; private set; }

    private Payment() { }

    public static Payment Create(Guid orderId, decimal amount, string customerEmail)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = amount,
            CustomerEmail = customerEmail,
            ProcessedAtUtc = DateTime.UtcNow
        };
    }
}
