namespace PaymentService.API.Dtos;

public record PaymentResponseDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string CustomerEmail,
    DateTime ProcessedAtUtc);