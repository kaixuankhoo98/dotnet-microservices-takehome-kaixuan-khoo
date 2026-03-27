using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentService.API.Application.Dtos;
using PaymentService.API.Application.Services;

namespace PaymentService.API.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentAppService paymentAppService;

        public PaymentsController(IPaymentAppService paymentAppService)
        {
            this.paymentAppService = paymentAppService;
        }

        /// <summary>
        /// Gets all processed payments.
        /// </summary>
        /// <response code="200">Returns the list of processed payments.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<PaymentResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<PaymentResponseDto>>> GetProcessedPayments(CancellationToken ct)
        {
            var payments = await paymentAppService.GetAllPaymentsAsync(ct);
            return Ok(payments);
        }
    }
}
