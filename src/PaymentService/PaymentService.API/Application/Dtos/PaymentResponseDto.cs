namespace PaymentService.API.Application.Dtos;

public record PaymentResponseDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string CustomerEmail,
    DateTime ProcessedAtUtc);