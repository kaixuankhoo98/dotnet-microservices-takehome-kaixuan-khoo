namespace OrderService.API.Dtos;

public record OrderResponseDto(
    Guid Id,
    decimal Amount,
    string CustomerEmail,
    DateTime CreatedAtUtc);