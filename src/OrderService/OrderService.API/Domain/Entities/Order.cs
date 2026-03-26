using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace OrderService.API.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }

    [Precision(18,2)]
    public decimal Amount { get; private set; }

    [MaxLength(256)]
    public string CustomerEmail { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    private Order() { }

    public static Order Create(decimal amount, string customerEmail)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than 0", nameof(amount));
        }

        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            throw new ArgumentException("Customer email is required", nameof(customerEmail));
        }

        return new Order
        {
            Id = Guid.NewGuid(),
            Amount = amount,
            CustomerEmail = customerEmail,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
