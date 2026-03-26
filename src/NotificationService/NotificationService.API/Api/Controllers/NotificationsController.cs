using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotificationService.API.Application.Dtos;
using NotificationService.API.Application.Services;

namespace NotificationService.API.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationAppService notificationAppService;

        public NotificationsController(INotificationAppService notificationAppService)
        {
            this.notificationAppService = notificationAppService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyCollection<NotificationResponseDto>>> GetAllAsync(CancellationToken ct)
        {
            var notifications = await this.notificationAppService.GetAllNotificationsAsync(ct);
            return Ok(notifications);
        }
    }
}
