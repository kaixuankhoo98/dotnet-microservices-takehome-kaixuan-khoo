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

        /// <summary>
        /// Gets all generated notifications.
        /// </summary>
        /// <response code="200">Returns the list of notifications.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<NotificationResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyCollection<NotificationResponseDto>>> GetAllAsync(CancellationToken ct)
        {
            var notifications = await this.notificationAppService.GetAllNotificationsAsync(ct);
            return Ok(notifications);
        }
    }
}
