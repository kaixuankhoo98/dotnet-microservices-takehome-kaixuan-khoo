using PaymentService.API.Dtos;

namespace PaymentService.API.Services;

public interface IPaymentAppService
{
    Task<PaymentResponseDto> ProcessOrderCreatedAsync(
        Guid orderId,
        decimal amount,
        string customerEmail,
        Guid correlationId,
        CancellationToken ct);

    Task<IReadOnlyList<PaymentResponseDto>> GetAllPaymentsAsync(CancellationToken ct);
}
