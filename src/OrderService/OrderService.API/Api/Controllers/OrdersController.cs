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

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrderResponseDto>>> GetOrders(CancellationToken ct)
        {
            var orders = await orderAppService.GetAllOrdersAsync(ct);
            return Ok(orders);
        }

        [HttpPost]
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
