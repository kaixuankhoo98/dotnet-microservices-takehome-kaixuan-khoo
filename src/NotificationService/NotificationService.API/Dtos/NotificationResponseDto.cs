namespace NotificationService.API.Dtos;

public record NotificationResponseDto(
    string Message,
    DateTime SentAtUtc);