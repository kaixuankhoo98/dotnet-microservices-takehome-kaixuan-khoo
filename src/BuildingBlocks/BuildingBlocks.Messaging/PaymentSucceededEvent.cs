namespace BuildingBlocks.Messaging;

public record PaymentSucceededEvent
{
    public Guid OrderId { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public DateTime TimeStamp { get; init; }
    public Guid CorrelationId { get; init; }
}