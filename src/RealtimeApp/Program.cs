// Demo mode: InMemory database with seed data loaded on startup
using RealtimeApp.Hubs;
using RealtimeApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.MaximumReceiveMessageSize = 64 * 1024; // 64KB
}).AddMessagePackProtocol();

builder.Services.AddSingleton<ConnectionTracker>();

builder.Services.AddCors(options => options.AddDefaultPolicy(b =>
    b.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true).AllowCredentials()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<DashboardHub>("/hubs/dashboard");

app.MapGet("/", () => Results.Ok(new
{
    service = "SignalR Realtime",
    hubs = new[] { "/hubs/chat", "/hubs/notifications", "/hubs/dashboard" },
    docs = "/swagger",
}));

app.Run();
