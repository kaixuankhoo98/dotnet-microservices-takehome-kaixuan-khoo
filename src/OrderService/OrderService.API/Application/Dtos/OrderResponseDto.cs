namespace OrderService.API.Application.Dtos;

public record OrderResponseDto(
    Guid Id,
    decimal Amount,
    string CustomerEmail,
    DateTime CreatedAtUtc);