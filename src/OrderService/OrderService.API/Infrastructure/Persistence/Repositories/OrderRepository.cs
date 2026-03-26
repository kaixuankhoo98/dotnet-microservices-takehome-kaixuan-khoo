using Microsoft.EntityFrameworkCore;
using OrderService.API.Domain.Entities;
using OrderService.API.Infrastructure.Data;

namespace OrderService.API.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext orderDbContext;

    public OrderRepository(OrderDbContext orderDbContext)
    {
        this.orderDbContext = orderDbContext;
    }

    public async Task<Order> AddAsync(Order order, CancellationToken ct = default)
    {
        await orderDbContext.Set<Order>().AddAsync(order, ct);
        return order;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await orderDbContext
            .Set<Order>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default)
    {
        return await orderDbContext
            .Set<Order>()
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return orderDbContext.SaveChangesAsync(ct);
    }
}