using OrderService.API.Domain.Entities;

namespace OrderService.API.Infrastructure.Persistence.Repositories;
public interface IOrderRepository
{
    Task<Order> AddAsync(Order order, CancellationToken ct = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
