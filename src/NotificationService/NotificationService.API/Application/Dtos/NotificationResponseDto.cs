namespace NotificationService.API.Application.Dtos;

public record NotificationResponseDto(
    string Message,
    DateTime SentAtUtc);