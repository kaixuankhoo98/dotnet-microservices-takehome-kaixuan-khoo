using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.Application.Dtos;
using OrderService.API.Application.Services;

namespace OrderService.API.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderAppService orderAppService;

        public OrdersController(IOrderAppService orderAppService)
        {
            this.orderAppService = orderAppService;
        }

        /// <summary>
        /// Gets all orders.
        /// </summary>
        /// <response code="200">Returns the list of orders.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<OrderResponseDto>>> GetOrders(CancellationToken ct)
        {
            var orders = await orderAppService.GetAllOrdersAsync(ct);
            return Ok(orders);
        }

        /// <summary>
        /// Creates a new order and publishes an OrderCreatedEvent.
        /// </summary>
        /// <response code="200">Returns the created order.</response>
        /// <response code="400">Returns when validation fails.</response>
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder(
            [FromBody] CreateOrderRequestDto request,
            CancellationToken ct
        )
        {
            var correlationId = HttpContext.Items["CorrelationId"] is string cid
                ? Guid.TryParse(cid, out var parsed) ? parsed : Guid.NewGuid()
                : Guid.NewGuid();

            var result = await orderAppService.CreateOrderAsync(request, correlationId, ct);

            return Ok(result);
        }
    }
}
