# dotnet-signalr-realtime

ASP.NET Core 8 real-time platform with 3 SignalR hubs: ChatHub (rooms, typing, presence, DMs), NotificationHub (per-user push, topics, broadcasts), and DashboardHub (live metric streaming). Includes server-side push via REST API, connection tracking, and MessagePack protocol support.

## Stack

- **Framework:** ASP.NET Core 8
- **Real-time:** SignalR with MessagePack binary protocol
- **Docs:** Swagger/OpenAPI

## SignalR Hubs

### ChatHub (`/hubs/chat`)

| Client Method | Server Event | Description |
|---|---|---|
| `JoinRoom(room)` | `UserJoinedRoom`, `RoomMembers` | Join a chat room, receive member list |
| `LeaveRoom(room)` | `UserLeftRoom` | Leave a room |
| `SendMessage(room, content)` | `ReceiveMessage` | Send message to room |
| `SendTyping(room, isTyping)` | `UserTyping` | Typing indicator |
| `UpdateStatus(status)` | `PresenceUpdated` | Change online/away/offline status |
| `SendDirectMessage(userId, content)` | `DirectMessage` | Private message to user |
| — (on connect) | `UserConnected`, `OnlineUsers` | Auto-presence on connection |
| — (on disconnect) | `UserDisconnected` | Auto-offline on disconnect |

### NotificationHub (`/hubs/notifications`)

| Client Method | Server Event | Description |
|---|---|---|
| `SubscribeToTopic(topic)` | — | Join topic group for targeted notifications |
| `UnsubscribeFromTopic(topic)` | — | Leave topic group |
| — | `NotificationReceived` | Receive notification (via REST push) |

### DashboardHub (`/hubs/dashboard`)

| Client Method | Server Event | Description |
|---|---|---|
| `SubscribeToMetric(name)` | — | Subscribe to specific metric updates |
| — | `MetricUpdated` | Receive live metric values |

## REST API (Server-Side Push)

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/notifications/push` | Push to specific user |
| POST | `/api/notifications/broadcast` | Push to all connected clients |
| POST | `/api/notifications/topic/{topic}` | Push to topic subscribers |
| POST | `/api/dashboard/metrics` | Push metric update to dashboard |
| GET | `/api/connections/online` | List online users with status |
| GET | `/api/connections/rooms/{room}` | List room members |

## Architecture

```
src/RealtimeApp/
├── Hubs/
│   ├── ChatHub.cs                 # Rooms, messages, typing, presence, DMs
│   └── NotificationHub.cs        # Notification + Dashboard hubs
├── Models/Models.cs               # ChatMessage, NotificationMessage, DashboardMetric, etc.
├── Services/ConnectionTracker.cs  # ConcurrentDictionary-based connection state
├── Controllers/Controllers.cs     # REST endpoints for server-side push
├── Program.cs                     # SignalR config, hub mapping, CORS
└── RealtimeApp.csproj
```

## Connection Tracking

The `ConnectionTracker` service maintains in-memory state using `ConcurrentDictionary` for thread safety. Tracks: connection ID, user ID, display name, status (online/away/offline), rooms joined, and connection timestamp. Supports multi-device (same user, multiple connections).

## Setup

```bash
git clone https://github.com/Shaisolaris/dotnet-signalr-realtime.git
cd dotnet-signalr-realtime
dotnet run --project src/RealtimeApp
# → Hub endpoints: /hubs/chat, /hubs/notifications, /hubs/dashboard
# → REST API: /swagger
```

## Key Design Decisions

**Three separate hubs.** Chat, notifications, and dashboard are distinct SignalR hubs with different concerns. Clients connect to only the hubs they need. This reduces per-connection overhead and enables independent scaling.

**Connection tracker as singleton.** `ConnectionTracker` uses `ConcurrentDictionary` for thread-safe, lock-free reads. In production with multiple servers, replace with Redis-backed distributed tracking.

**REST API for server-side push.** Backend services push notifications and metrics via REST controllers that inject `IHubContext`. This decouples event producers from SignalR clients. Any service with HTTP access can trigger real-time updates.

**MessagePack protocol.** MessagePack binary serialization reduces payload size by ~30% vs JSON. Both JSON and MessagePack protocols are available; clients negotiate during handshake.

**Group-based routing.** Users join groups for rooms (`room_name`), user channels (`user_{id}`), topics (`topic_{name}`), and dashboards (`dashboard_{id}`). SignalR's group management handles connection lifecycle automatically.

## License

MIT
