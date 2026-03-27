using BuildingBlocks.Messaging.Events;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.API.Application.Dtos;
using NotificationService.API.Application.Services;
using NotificationService.API.Domain.Entities;
using NotificationService.API.Infrastructure.Messaging.Consumers;
using NotificationService.API.Infrastructure.Persistence.Repositories;

namespace NotificationService.Tests;

public class NotificationAppServiceTests
{
    [Fact]
    public async Task PaymentSucceededConsumer_WhenMessageIsInvalid_DoesNotCallProcessPaymentSucceededAsync()
    {
        // Arrange
        var message = new PaymentSucceededEvent
        {
            OrderId = Guid.NewGuid(),
            PaymentId = Guid.Empty,
            Amount = 49.99m,
            CustomerEmail = "customer@example.com",
            CorrelationId = Guid.NewGuid(),
            TimeStamp = DateTime.UtcNow
        };

        var notificationAppService = new Mock<INotificationAppService>();
        var logger = new Mock<ILogger<PaymentSucceededConsumer>>();
        var context = new Mock<ConsumeContext<PaymentSucceededEvent>>();
        context.SetupGet(c => c.Message).Returns(message);
        context.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);

        var sut = new PaymentSucceededConsumer(notificationAppService.Object, logger.Object);

        // Act
        await sut.Consume(context.Object);

        // Assert
        notificationAppService.Verify(
            service => service.ProcessPaymentSucceededAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessPaymentSucceededAsync_WhenNotificationAlreadyExists_ReturnsExistingNotification_AndDoesNotCreateNewNotification()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var amount = 12.99m;
        var customerEmail = "existing@example.com";
        var correlationId = Guid.NewGuid();
        var ct = CancellationToken.None;

        var existingNotification = Notification.Create(orderId, paymentId, amount, customerEmail);

        var notificationRepository = new Mock<INotificationRepository>();
        notificationRepository
            .Setup(repo => repo.GetByPaymentIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingNotification);

        var logger = new Mock<ILogger<NotificationAppService>>();

        var sut = new NotificationAppService(notificationRepository.Object, logger.Object);

        // Act
        NotificationResponseDto result = await sut.ProcessPaymentSucceededAsync(
            paymentId,
            orderId,
            amount,
            customerEmail,
            correlationId,
            ct);

        // Assert
        result.Message.Should().Be(existingNotification.Message);
        result.SentAtUtc.Should().Be(existingNotification.SentAtUtc);

        notificationRepository.Verify(
            repo => repo.GetByPaymentIdAsync(paymentId, ct),
            Times.Once);
        notificationRepository.Verify(
            repo => repo.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessPaymentSucceededAsync_WhenNotificationDoesNotExist_CreatesNotificationAndSaves()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var amount = 12.99m;
        var customerEmail = "customer@example.com";
        var correlationId = Guid.NewGuid();
        var ct = CancellationToken.None;

        Notification? capturedNotification = null;

        var notificationRepository = new Mock<INotificationRepository>();
        notificationRepository
            .Setup(repo => repo.GetByPaymentIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        notificationRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((notification, _) => capturedNotification = notification)
            .ReturnsAsync((Notification notification, CancellationToken _) => notification);

        var logger = new Mock<ILogger<NotificationAppService>>();
        var sut = new NotificationAppService(notificationRepository.Object, logger.Object);

        // Act
        var result = await sut.ProcessPaymentSucceededAsync(
            paymentId,
            orderId,
            amount,
            customerEmail,
            correlationId,
            ct);

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification!.OrderId.Should().Be(orderId);
        capturedNotification.PaymentId.Should().Be(paymentId);
        capturedNotification.Amount.Should().Be(amount);
        capturedNotification.CustomerEmail.Should().Be(customerEmail);

        result.Message.Should().Be(capturedNotification.Message);
        result.SentAtUtc.Should().Be(capturedNotification.SentAtUtc);

        notificationRepository.Verify(
            repo => repo.GetByPaymentIdAsync(paymentId, ct),
            Times.Once);
        notificationRepository.Verify(
            repo => repo.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllNotificationsAsync_MapsRepositoryEntitiesToDtos()
    {
        // Arrange
        var ct = CancellationToken.None;

        var notificationOne = Notification.Create(Guid.NewGuid(), Guid.NewGuid(), 10m, "one@example.com");
        var notificationTwo = Notification.Create(Guid.NewGuid(), Guid.NewGuid(), 20m, "two@example.com");

        var notifications = new List<Notification> { notificationOne, notificationTwo };

        var notificationRepository = new Mock<INotificationRepository>();
        notificationRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        var logger = new Mock<ILogger<NotificationAppService>>();
        var sut = new NotificationAppService(notificationRepository.Object, logger.Object);

        // Act
        var result = await sut.GetAllNotificationsAsync(ct);

        // Assert
        result.Should().HaveCount(2);
        var resultList = result.ToList();
        resultList[0].Message.Should().Be(notificationOne.Message);
        resultList[0].SentAtUtc.Should().Be(notificationOne.SentAtUtc);

        resultList[1].Message.Should().Be(notificationTwo.Message);
        resultList[1].SentAtUtc.Should().Be(notificationTwo.SentAtUtc);

        notificationRepository.Verify(repo => repo.GetAllAsync(ct), Times.Once);
        notificationRepository.Verify(
            repo => repo.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
