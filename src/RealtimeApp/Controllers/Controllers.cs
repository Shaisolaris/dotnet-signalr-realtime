namespace RealtimeApp.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RealtimeApp.Hubs;
using RealtimeApp.Models;
using RealtimeApp.Services;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IHubContext<NotificationHub> _hub;
    public NotificationsController(IHubContext<NotificationHub> hub) => _hub = hub;

    [HttpPost("push")]
    public async Task<IActionResult> Push([FromBody] PushNotificationRequest request)
    {
        var notification = new NotificationMessage(request.UserId, request.Type, request.Title, request.Body, request.Data, DateTime.UtcNow);
        await _hub.Clients.Group($"user_{request.UserId}").SendAsync("NotificationReceived", notification);
        return Ok(new { sent = true, userId = request.UserId });
    }

    [HttpPost("broadcast")]
    public async Task<IActionResult> Broadcast([FromBody] PushNotificationRequest request)
    {
        var notification = new NotificationMessage("system", request.Type, request.Title, request.Body, request.Data, DateTime.UtcNow);
        await _hub.Clients.All.SendAsync("NotificationReceived", notification);
        return Ok(new { sent = true, target = "all" });
    }

    [HttpPost("topic/{topic}")]
    public async Task<IActionResult> SendToTopic(string topic, [FromBody] PushNotificationRequest request)
    {
        var notification = new NotificationMessage("system", request.Type, request.Title, request.Body, request.Data, DateTime.UtcNow);
        await _hub.Clients.Group($"topic_{topic}").SendAsync("NotificationReceived", notification);
        return Ok(new { sent = true, topic });
    }
}

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IHubContext<DashboardHub> _hub;
    public DashboardController(IHubContext<DashboardHub> hub) => _hub = hub;

    [HttpPost("metrics")]
    public async Task<IActionResult> UpdateMetric([FromBody] UpdateMetricRequest request)
    {
        var metric = new DashboardMetric(request.MetricName, request.Value, null, request.Unit, DateTime.UtcNow);
        await _hub.Clients.Group($"metric_{request.MetricName}").SendAsync("MetricUpdated", metric);
        await _hub.Clients.Group("dashboard_default").SendAsync("MetricUpdated", metric);
        return Ok(new { sent = true, metric = request.MetricName });
    }
}

[ApiController]
[Route("api/[controller]")]
public class ConnectionsController : ControllerBase
{
    private readonly ConnectionTracker _tracker;
    public ConnectionsController(ConnectionTracker tracker) => _tracker = tracker;

    [HttpGet("online")]
    public IActionResult GetOnlineUsers() => Ok(new
    {
        count = _tracker.OnlineCount,
        users = _tracker.GetOnlineUsers().Select(u => new { u.UserId, u.UserName, u.Status, u.ConnectedAt, rooms = u.Rooms }),
    });

    [HttpGet("rooms/{room}")]
    public IActionResult GetRoomMembers(string room) => Ok(new
    {
        room,
        members = _tracker.GetRoomMembers(room).Select(u => new { u.UserId, u.UserName }),
    });
}
