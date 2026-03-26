using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentService.API.Dtos;
using PaymentService.API.Services;

namespace PaymentService.API.Controllers
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

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PaymentResponseDto>>> GetProcessedPayments(CancellationToken ct)
        {
            var payments = await this.paymentAppService.GetAllPaymentsAsync(ct);
            return Ok(payments);
        }
    }
}
