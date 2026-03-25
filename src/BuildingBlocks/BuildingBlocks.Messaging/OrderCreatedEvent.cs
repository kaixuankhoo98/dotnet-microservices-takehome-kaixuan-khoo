namespace BuildingBlocks.Messaging;

public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public Guid CorrelationId { get; init; }
}
