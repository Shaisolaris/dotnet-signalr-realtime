namespace RealtimeApp.Models;

public record ChatMessage(string UserId, string UserName, string Room, string Content, DateTime SentAt);
public record NotificationMessage(string UserId, string Type, string Title, string Body, object? Data, DateTime CreatedAt);
public record DashboardMetric(string MetricName, double Value, double? PreviousValue, string Unit, DateTime UpdatedAt);
public record PresenceUpdate(string UserId, string UserName, string Status, DateTime Timestamp);
public record TypingIndicator(string UserId, string UserName, string Room, bool IsTyping);

public record SendMessageRequest(string Room, string Content);
public record JoinRoomRequest(string Room);
public record PushNotificationRequest(string UserId, string Type, string Title, string Body, object? Data);
public record UpdateMetricRequest(string MetricName, double Value, string Unit);

public class ConnectedUser
{
    public string ConnectionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Status { get; set; } = "online";
    public HashSet<string> Rooms { get; set; } = new();
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
}
