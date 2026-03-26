using BuildingBlocks.Messaging.Events;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentService.API.Application.Services;
using PaymentService.API.Domain.Entities;
using PaymentService.API.Infrastructure.Persistence.Repositories;

namespace PaymentService.Tests;

public class PaymentAppServiceTests
{
    [Fact]
    public async Task ProcessOrderCreatedAsync_WhenPaymentAlreadyExists_ReturnsExistingPayment_DoesntCreateNewPayment()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var existingPayment = Payment.Create(orderId, 1.99m, "existing@example.com");

        var paymentRepository = new Mock<IPaymentRepository>();
        paymentRepository
            .Setup(repo => repo.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        var publishEndpoint = new Mock<IPublishEndpoint>();
        var logger = new Mock<ILogger<PaymentAppService>>();

        var sut = new PaymentAppService(paymentRepository.Object, publishEndpoint.Object, logger.Object);

        // Act
        var result = await sut.ProcessOrderCreatedAsync(
            orderId,
            2.99m,
            "ignored@example.com",
            correlationId,
            CancellationToken.None);

        // Assert
        result.Id.Should().Be(existingPayment.Id);
        result.OrderId.Should().Be(existingPayment.OrderId);
        result.Amount.Should().Be(existingPayment.Amount);
        result.CustomerEmail.Should().Be(existingPayment.CustomerEmail);

        paymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
        paymentRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        publishEndpoint.Verify(
            endpoint => endpoint.Publish(
                It.IsAny<PaymentSucceededEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessOrderCreatedAsync_WhenPaymentDoesNotExist_CreatesPaymentPublishesEventAndSaves()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var amount = 12.99m;
        var customerEmail = "customer@example.com";
        var correlationId = Guid.NewGuid();

        var paymentRepository = new Mock<IPaymentRepository>();
        paymentRepository
            .Setup(repo => repo.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);
        paymentRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment payment, CancellationToken _) => payment);
        paymentRepository
            .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var publishEndpoint = new Mock<IPublishEndpoint>();
        publishEndpoint
            .Setup(endpoint => endpoint.Publish(It.IsAny<PaymentSucceededEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var logger = new Mock<ILogger<PaymentAppService>>();
        var sut = new PaymentAppService(paymentRepository.Object, publishEndpoint.Object, logger.Object);

        Payment? capturedPayment = null;
        paymentRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Callback<Payment, CancellationToken>((payment, _) => capturedPayment = payment)
            .ReturnsAsync((Payment payment, CancellationToken _) => payment);

        PaymentSucceededEvent? capturedEvent = null;
        publishEndpoint
            .Setup(endpoint => endpoint.Publish(It.IsAny<PaymentSucceededEvent>(), It.IsAny<CancellationToken>()))
            .Callback<PaymentSucceededEvent, CancellationToken>((evt, _) => capturedEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        var result = await sut.ProcessOrderCreatedAsync(
            orderId,
            amount,
            customerEmail,
            correlationId,
            CancellationToken.None);

        // Assert
        capturedPayment.Should().NotBeNull();
        capturedPayment!.OrderId.Should().Be(orderId);
        capturedPayment.Amount.Should().Be(amount);
        capturedPayment.CustomerEmail.Should().Be(customerEmail);

        paymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
        paymentRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        capturedEvent.Should().NotBeNull();
        capturedEvent!.OrderId.Should().Be(orderId);
        capturedEvent.Amount.Should().Be(amount);
        capturedEvent.CustomerEmail.Should().Be(customerEmail);
        capturedEvent.CorrelationId.Should().Be(correlationId);
        capturedEvent.PaymentId.Should().Be(capturedPayment.Id);

        result.Id.Should().Be(capturedPayment.Id);
        result.OrderId.Should().Be(orderId);
        result.Amount.Should().Be(amount);
        result.CustomerEmail.Should().Be(customerEmail);
    }

    [Fact]
    public async Task GetAllPaymentsAsync_MapsRepositoryEntitiesToDtos()
    {
        // Arrange
        var paymentOne = Payment.Create(Guid.NewGuid(), 10m, "one@example.com");
        var paymentTwo = Payment.Create(Guid.NewGuid(), 20m, "two@example.com");
        var payments = new List<Payment> { paymentOne, paymentTwo };

        var paymentRepository = new Mock<IPaymentRepository>();
        paymentRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);

        var publishEndpoint = new Mock<IPublishEndpoint>();
        var logger = new Mock<ILogger<PaymentAppService>>();
        var sut = new PaymentAppService(paymentRepository.Object, publishEndpoint.Object, logger.Object);

        // Act
        var result = await sut.GetAllPaymentsAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        result[0].Id.Should().Be(paymentOne.Id);
        result[0].OrderId.Should().Be(paymentOne.OrderId);
        result[0].Amount.Should().Be(paymentOne.Amount);
        result[0].CustomerEmail.Should().Be(paymentOne.CustomerEmail);

        result[1].Id.Should().Be(paymentTwo.Id);
        result[1].OrderId.Should().Be(paymentTwo.OrderId);
        result[1].Amount.Should().Be(paymentTwo.Amount);
        result[1].CustomerEmail.Should().Be(paymentTwo.CustomerEmail);
    }
}
