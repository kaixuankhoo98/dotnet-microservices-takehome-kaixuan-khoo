using BuildingBlocks.Messaging.Events;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.API.Application.Dtos;
using OrderService.API.Application.Services;
using OrderService.API.Domain.Entities;
using OrderService.API.Infrastructure.Persistence.Repositories;

namespace OrderService.Tests;

public class OrderAppServiceTests
{
    [Fact]
    public async Task CreateOrderAsync_WhenValidInput_AddsOrderPublishesEventAndSavesChanges()
    {
        // Arrange
        var request = new CreateOrderRequestDto(12.99m, "customer@example.com");
        var correlationId = Guid.NewGuid();
        using var ctSource = new CancellationTokenSource();
        var ct = ctSource.Token;

        Order? capturedOrder = null;
        OrderCreatedEvent? capturedEvent = null;

        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => capturedOrder = order)
            .ReturnsAsync((Order order, CancellationToken _) => order);

        orderRepository
            .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var publishEndpoint = new Mock<IPublishEndpoint>();
        publishEndpoint
            .Setup(endpoint => endpoint.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<OrderCreatedEvent, CancellationToken>((evt, _) => capturedEvent = evt)
            .Returns(Task.CompletedTask);

        var logger = new Mock<ILogger<OrderAppService>>();

        var sut = new OrderAppService(orderRepository.Object, publishEndpoint.Object, logger.Object);

        // Act
        var result = await sut.CreateOrderAsync(request, correlationId, ct);

        // Assert
        capturedOrder.Should().NotBeNull();
        result.Id.Should().Be(capturedOrder.Id);
        result.Amount.Should().Be(request.Amount);
        result.CustomerEmail.Should().Be(request.CustomerEmail);
        result.CreatedAtUtc.Should().Be(capturedOrder.CreatedAtUtc);

        capturedEvent.Should().NotBeNull();
        capturedEvent!.OrderId.Should().Be(capturedOrder.Id);
        capturedEvent.Amount.Should().Be(capturedOrder.Amount);
        capturedEvent.CustomerEmail.Should().Be(capturedOrder.CustomerEmail);
        capturedEvent.CreatedAtUtc.Should().Be(capturedOrder.CreatedAtUtc);
        capturedEvent.CorrelationId.Should().Be(correlationId);

        orderRepository.Verify(
            repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Once);
        orderRepository.Verify(
            repo => repo.SaveChangesAsync(ct),
            Times.Once);

        publishEndpoint.Verify(
            endpoint => endpoint.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllOrdersAsync_MapsRepositoryEntitiesToDtos()
    {
        // Arrange
        var orderOne = Order.Create(10m, "one@example.com");
        var orderTwo = Order.Create(20m, "two@example.com");
        var orders = new List<Order> { orderOne, orderTwo };

        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var publishEndpoint = new Mock<IPublishEndpoint>();
        var logger = new Mock<ILogger<OrderAppService>>();

        var sut = new OrderAppService(orderRepository.Object, publishEndpoint.Object, logger.Object);

        // Act
        var result = await sut.GetAllOrdersAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(orderOne.Id);
        result[0].Amount.Should().Be(orderOne.Amount);
        result[0].CustomerEmail.Should().Be(orderOne.CustomerEmail);
        result[0].CreatedAtUtc.Should().Be(orderOne.CreatedAtUtc);

        result[1].Id.Should().Be(orderTwo.Id);
        result[1].Amount.Should().Be(orderTwo.Amount);
        result[1].CustomerEmail.Should().Be(orderTwo.CustomerEmail);
        result[1].CreatedAtUtc.Should().Be(orderTwo.CreatedAtUtc);

        publishEndpoint.Verify(
            endpoint => endpoint.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        orderRepository.Verify(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateOrderAsync_WhenAmountIsNotPositive_ThrowsArgumentExceptionAndDoesNotPersistOrPublish(decimal amount)
    {
        // Arrange
        var request = new CreateOrderRequestDto(amount, "customer@example.com");
        var correlationId = Guid.NewGuid();

        var orderRepository = new Mock<IOrderRepository>();
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var logger = new Mock<ILogger<OrderAppService>>();

        var sut = new OrderAppService(orderRepository.Object, publishEndpoint.Object, logger.Object);

        // Act + Assert
        Func<Task> act = () =>
            sut.CreateOrderAsync(request, correlationId, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();

        orderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        orderRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        publishEndpoint.Verify(endpoint => endpoint.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateOrderAsync_WhenCustomerEmailIsBlank_ThrowsArgumentExceptionAndDoesNotPersistOrPublish(string customerEmail)
    {
        // Arrange
        var request = new CreateOrderRequestDto(10m, customerEmail);
        var correlationId = Guid.NewGuid();

        var orderRepository = new Mock<IOrderRepository>();
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var logger = new Mock<ILogger<OrderAppService>>();

        var sut = new OrderAppService(orderRepository.Object, publishEndpoint.Object, logger.Object);

        // Act + Assert
        Func<Task> act = () =>
            sut.CreateOrderAsync(request, correlationId, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();

        orderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        orderRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        publishEndpoint.Verify(endpoint => endpoint.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
