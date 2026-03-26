using PaymentService.API.Domain.Entities;

namespace PaymentService.API.Infrastructure.Persistence.Repositories;
public interface IPaymentRepository
{
    Task<Payment> AddAsync(Payment payment, CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken ct = default);
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
