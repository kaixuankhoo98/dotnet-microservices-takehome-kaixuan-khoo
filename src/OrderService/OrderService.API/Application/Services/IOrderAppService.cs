using OrderService.API.Application.Dtos;

namespace OrderService.API.Application.Services;
public interface IOrderAppService
{
    public Task<OrderResponseDto> CreateOrderAsync(
        CreateOrderRequestDto request, Guid correlationId, CancellationToken ct);
    public Task<IReadOnlyList<OrderResponseDto>> GetAllOrdersAsync(
        CancellationToken ct);
}
