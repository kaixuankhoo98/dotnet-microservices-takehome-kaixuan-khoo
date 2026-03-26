using Microsoft.EntityFrameworkCore;
using PaymentService.API.Data;
using PaymentService.API.Entities;

namespace PaymentService.API.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext paymentDbContext;

    public PaymentRepository(PaymentDbContext paymentDbContext)
    {
        this.paymentDbContext = paymentDbContext;
    }

    public async Task<Payment> AddAsync(Payment payment, CancellationToken ct = default)
    {
        await this.paymentDbContext.Set<Payment>().AddAsync(payment, ct);
        return payment;
    }

    public async Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken ct = default)
    {
        return await this.paymentDbContext
            .Set<Payment>()
            .AsNoTracking()
            .OrderByDescending(p => p.ProcessedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await this.paymentDbContext
            .Set<Payment>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrderId == orderId, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return this.paymentDbContext.SaveChangesAsync(ct);
    }
}