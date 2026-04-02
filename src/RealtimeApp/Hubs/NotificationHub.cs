namespace RealtimeApp.Hubs;

using Microsoft.AspNetCore.SignalR;
using RealtimeApp.Models;

public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    public NotificationHub(ILogger<NotificationHub> logger) => _logger = logger;

    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await base.OnConnectedAsync();
    }

    public async Task SubscribeToTopic(string topic) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"topic_{topic}");

    public async Task UnsubscribeFromTopic(string topic) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"topic_{topic}");
}

public class DashboardHub : Hub
{
    private readonly ILogger<DashboardHub> _logger;
    public DashboardHub(ILogger<DashboardHub> logger) => _logger = logger;

    public override async Task OnConnectedAsync()
    {
        var dashboardId = Context.GetHttpContext()?.Request.Query["dashboardId"].ToString() ?? "default";
        await Groups.AddToGroupAsync(Context.ConnectionId, $"dashboard_{dashboardId}");
        _logger.LogInformation("Client joined dashboard: {DashboardId}", dashboardId);
        await base.OnConnectedAsync();
    }

    public async Task SubscribeToMetric(string metricName) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"metric_{metricName}");
}
