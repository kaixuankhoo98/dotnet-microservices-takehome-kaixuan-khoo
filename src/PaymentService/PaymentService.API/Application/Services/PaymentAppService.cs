using BuildingBlocks.Messaging.Events;
using MassTransit;
using PaymentService.API.Application.Dtos;
using PaymentService.API.Domain.Entities;
using PaymentService.API.Infrastructure.Persistence.Repositories;

namespace PaymentService.API.Application.Services;

public class PaymentAppService : IPaymentAppService
{
    private readonly IPaymentRepository paymentRepository;
    private readonly IPublishEndpoint publishEndpoint;
    private readonly ILogger<PaymentAppService> logger;

    public PaymentAppService(
        IPaymentRepository paymentRepository,
        IPublishEndpoint publishEndpoint,
        ILogger<PaymentAppService> logger)
    {
        this.paymentRepository = paymentRepository;
        this.publishEndpoint = publishEndpoint;
        this.logger = logger;
    }

    public async Task<PaymentResponseDto> ProcessOrderCreatedAsync(
        Guid orderId,
        decimal amount,
        string customerEmail,
        Guid correlationId,
        CancellationToken ct)
    {
        var existing = await paymentRepository.GetByOrderIdAsync(orderId, ct);
        if (existing != null)
        {
            logger.LogInformation(
                "Payment already exists for Order {OrderId}, skipping publish",
                orderId);
            return new PaymentResponseDto(
                existing.Id,
                existing.OrderId,
                existing.Amount,
                existing.CustomerEmail,
                existing.ProcessedAtUtc);
        }

        // Demo only: pretend we called an external PSP / card network
        await Task.Delay(TimeSpan.FromMilliseconds(1500), ct);

        var payment = Payment.Create(orderId, amount, customerEmail);
        await paymentRepository.AddAsync(payment, ct);

        logger.LogInformation(
            "Payment {PaymentId} created for Order {OrderId}, Amount: {Amount}",
            payment.Id, orderId, amount);

        await publishEndpoint.Publish(new PaymentSucceededEvent
        {
            OrderId = orderId,
            PaymentId = payment.Id,
            Amount = amount,
            CustomerEmail = customerEmail,
            TimeStamp = DateTime.UtcNow,
            CorrelationId = correlationId,
        });

        logger.LogInformation(
            "PaymentSucceededEvent published for Order {OrderId}, Payment {PaymentId}",
            orderId, payment.Id);

        await paymentRepository.SaveChangesAsync(ct);

        return new PaymentResponseDto(
            payment.Id,
            payment.OrderId,
            payment.Amount,
            payment.CustomerEmail,
            payment.ProcessedAtUtc);
    }

    public async Task<IReadOnlyList<PaymentResponseDto>> GetAllPaymentsAsync(CancellationToken ct)
    {
        var payments = await paymentRepository.GetAllAsync(ct);

        return payments.Select(p => new PaymentResponseDto(
                p.Id,
                p.OrderId,
                p.Amount,
                p.CustomerEmail,
                p.ProcessedAtUtc
            )).ToList();
    }
}
