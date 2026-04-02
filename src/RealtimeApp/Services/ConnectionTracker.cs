namespace RealtimeApp.Services;

using System.Collections.Concurrent;
using RealtimeApp.Models;

public class ConnectionTracker
{
    private readonly ConcurrentDictionary<string, ConnectedUser> _connections = new();

    public void AddConnection(string connectionId, string userId, string userName)
    {
        _connections[connectionId] = new ConnectedUser
        {
            ConnectionId = connectionId, UserId = userId, UserName = userName,
        };
    }

    public void RemoveConnection(string connectionId) => _connections.TryRemove(connectionId, out _);

    public ConnectedUser? GetByConnectionId(string connectionId) =>
        _connections.TryGetValue(connectionId, out var user) ? user : null;

    public IEnumerable<ConnectedUser> GetByUserId(string userId) =>
        _connections.Values.Where(u => u.UserId == userId);

    public IEnumerable<ConnectedUser> GetOnlineUsers() => _connections.Values;

    public int OnlineCount => _connections.Count;

    public void JoinRoom(string connectionId, string room)
    {
        if (_connections.TryGetValue(connectionId, out var user))
            user.Rooms.Add(room);
    }

    public void LeaveRoom(string connectionId, string room)
    {
        if (_connections.TryGetValue(connectionId, out var user))
            user.Rooms.Remove(room);
    }

    public IEnumerable<ConnectedUser> GetRoomMembers(string room) =>
        _connections.Values.Where(u => u.Rooms.Contains(room));

    public void UpdateStatus(string connectionId, string status)
    {
        if (_connections.TryGetValue(connectionId, out var user))
            user.Status = status;
    }
}
