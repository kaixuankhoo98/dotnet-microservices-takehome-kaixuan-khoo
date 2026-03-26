using Microsoft.EntityFrameworkCore;
using PaymentService.API.Domain.Entities;
using PaymentService.API.Infrastructure.Data;

namespace PaymentService.API.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext paymentDbContext;

    public PaymentRepository(PaymentDbContext paymentDbContext)
    {
        this.paymentDbContext = paymentDbContext;
    }

    public async Task<Payment> AddAsync(Payment payment, CancellationToken ct = default)
    {
        await paymentDbContext.Set<Payment>().AddAsync(payment, ct);
        return payment;
    }

    public async Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken ct = default)
    {
        return await paymentDbContext
            .Set<Payment>()
            .AsNoTracking()
            .OrderByDescending(p => p.ProcessedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await paymentDbContext
            .Set<Payment>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrderId == orderId, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return paymentDbContext.SaveChangesAsync(ct);
    }
}