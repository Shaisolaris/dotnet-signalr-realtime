namespace RealtimeApp.Hubs;

using Microsoft.AspNetCore.SignalR;
using RealtimeApp.Models;
using RealtimeApp.Services;

public class ChatHub : Hub
{
    private readonly ConnectionTracker _tracker;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ConnectionTracker tracker, ILogger<ChatHub> logger)
    {
        _tracker = tracker;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString() ?? Context.ConnectionId;
        var userName = Context.GetHttpContext()?.Request.Query["userName"].ToString() ?? "Anonymous";

        _tracker.AddConnection(Context.ConnectionId, userId, userName);

        await Clients.All.SendAsync("UserConnected", new PresenceUpdate(userId, userName, "online", DateTime.UtcNow));
        await Clients.Caller.SendAsync("OnlineUsers", _tracker.GetOnlineUsers().Select(u => new { u.UserId, u.UserName, u.Status }));

        _logger.LogInformation("User {UserName} ({UserId}) connected", userName, userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = _tracker.GetByConnectionId(Context.ConnectionId);
        if (user != null)
        {
            foreach (var room in user.Rooms.ToList())
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);

            await Clients.All.SendAsync("UserDisconnected", new PresenceUpdate(user.UserId, user.UserName, "offline", DateTime.UtcNow));
            _logger.LogInformation("User {UserName} disconnected", user.UserName);
        }

        _tracker.RemoveConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(string room)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, room);
        _tracker.JoinRoom(Context.ConnectionId, room);

        var user = _tracker.GetByConnectionId(Context.ConnectionId);
        if (user != null)
        {
            await Clients.Group(room).SendAsync("UserJoinedRoom", new { user.UserId, user.UserName, Room = room });
            await Clients.Caller.SendAsync("RoomMembers", room, _tracker.GetRoomMembers(room).Select(u => new { u.UserId, u.UserName }));
        }
    }

    public async Task LeaveRoom(string room)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
        _tracker.LeaveRoom(Context.ConnectionId, room);

        var user = _tracker.GetByConnectionId(Context.ConnectionId);
        if (user != null)
            await Clients.Group(room).SendAsync("UserLeftRoom", new { user.UserId, user.UserName, Room = room });
    }

    public async Task SendMessage(string room, string content)
    {
        var user = _tracker.GetByConnectionId(Context.ConnectionId);
        if (user == null) return;

        var message = new ChatMessage(user.UserId, user.UserName, room, content, DateTime.UtcNow);
        await Clients.Group(room).SendAsync("ReceiveMessage", message);
        _logger.LogDebug("Message in {Room} from {User}: {Content}", room, user.UserName, content[..Math.Min(50, content.Length)]);
    }

    public async Task SendTyping(string room, bool isTyping)
    {
        var user = _tracker.GetByConnectionId(Context.ConnectionId);
        if (user == null) return;

        await Clients.OthersInGroup(room).SendAsync("UserTyping", new TypingIndicator(user.UserId, user.UserName, room, isTyping));
    }

    public async Task UpdateStatus(string status)
    {
        _tracker.UpdateStatus(Context.ConnectionId, status);
        var user = _tracker.GetByConnectionId(Context.ConnectionId);
        if (user != null)
            await Clients.All.SendAsync("PresenceUpdated", new PresenceUpdate(user.UserId, user.UserName, status, DateTime.UtcNow));
    }

    public async Task SendDirectMessage(string targetUserId, string content)
    {
        var sender = _tracker.GetByConnectionId(Context.ConnectionId);
        if (sender == null) return;

        var targets = _tracker.GetByUserId(targetUserId);
        foreach (var target in targets)
        {
            await Clients.Client(target.ConnectionId).SendAsync("DirectMessage", new
            {
                sender.UserId, sender.UserName, Content = content, SentAt = DateTime.UtcNow,
            });
        }
    }
}
