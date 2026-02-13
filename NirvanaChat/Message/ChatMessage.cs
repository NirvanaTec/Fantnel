using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Codexus.Development.SDK.Connection;
using NirvanaAPI.Entities.EntitiesNirvana;
using NirvanaChat.Entities;
using NirvanaChat.Manager;
using Serilog;

namespace NirvanaChat.Message;

public static class ChatMessage {
    private static readonly List<EntityChatJoin> Players = [];

    private static readonly Uri Url = new("ws://110.42.70.32:13423/ws/chat");
    public static EntityChatConfig Config = new();

    private static bool _friendlyMode; // 友好模式

    private static volatile bool _enabled; // 是否启用

    private static Timer? _heartbeat; // 心跳定时器
    private static ClientWebSocket _webSocket = new();

    public static EntityNirvanaAccount Account { get; set; } = new();

    public static void Start()
    {
        _ = StartAsync();
    }

    public static void Join(GameConnection gameConnection, int entityId)
    {
        var isAdd = true;
        foreach (var player in Players.Where(player => player.Equals(gameConnection))) {
            player.EntityId = entityId;
            isAdd = false;
            break;
        }

        var entity = new EntityChatJoin {
            EntityId = entityId,
            GameId = gameConnection.GameId,
            NickName = gameConnection.NickName
        };

        if (isAdd) {
            Players.Add(entity);
        }

        var message = JsonSerializer.Serialize(entity);
        SendAsync(message).Wait();
    }

    private static void Join(EntityChatJoin entityChatJoin)
    {
        if (Players.Any(player => player.Equals(entityChatJoin))) {
            return;
        }

        Players.Add(entityChatJoin);
        var message = JsonSerializer.Serialize(entityChatJoin);
        SendAsync(message).Wait();
    }

    public static void RemoveJoin(GameConnection gameConnection)
    {
        foreach (var player in Players.Where(player => player.Equals(gameConnection)).ToList()) {
            Players.Remove(player);
        }

        var entity = new EntityChatJoin {
            Mode = "removeJoin",
            GameId = gameConnection.GameId,
            NickName = gameConnection.NickName
        };
        var message = JsonSerializer.Serialize(entity);
        SendAsync(message).Wait();
    }

    private static async Task StartAsync()
    {
        _enabled = true;

        await _webSocket.ConnectAsync(Url, CancellationToken.None);
        Log.Information("[IRC] 连接成功!");

        if (Account.IsNullOrEmpty()) {
            await RefreshChatConfigAsync();
        } else {
            await AuthenticateAsync();
        }

        new Thread(Initialize).Start();

        var interval = TimeSpan.FromMinutes(5);
        _heartbeat = new Timer(OnHeartbeat, null, TimeSpan.Zero, interval);

        foreach (var player in Players) {
            Join(player);
        }
    }

    private static void OnHeartbeat(object? state)
    {
        try {
            if (Config.Heartbeats.Count == 0) {
                return;
            }

            foreach (var gameConnection in ChatManager.List) {
                // 随机取 IrcInfo.Heartbeats
                var heartbeat = Config.GetHeartbeat();
                if (string.IsNullOrEmpty(heartbeat)) {
                    return;
                }

                PacketTools.SendGameMessage(heartbeat, gameConnection);
            }
        } catch (Exception e) {
            Log.Error("[IRC] 心跳失败\n{message}", e.Message);
        }
    }

    public static void Shutdown()
    {
        ShutdownAsync().Wait();
    }

    private static async Task ShutdownAsync()
    {
        _enabled = false;
        if (_heartbeat != null) {
            try {
                await _heartbeat.DisposeAsync();
            } catch (Exception e) {
                Log.Error("[IRC] 关闭心跳失败\n{message}", e.Message);
            }
        }

        try {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
        } catch (Exception e) {
            Log.Error("[IRC] 关闭连接失败\n{message}", e.Message);
        }

        _webSocket = new ClientWebSocket();
    }

    private static void Initialize()
    {
        InitializeAsync().Wait();
    }

    private static async Task InitializeAsync()
    {
        var buffer = new byte[1024 * 4]; // 4KB 缓冲区

        while (_enabled && _webSocket.State == WebSocketState.Open) {
            try {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close) {
                    Log.Error("[IRC] 服务器要求关闭连接。");
                    Shutdown();
                    Start();
                    return;
                }

                if (result.Count == 0) {
                    continue;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var jsonMsg = JsonSerializer.Deserialize<EntityMode>(message);

                switch (jsonMsg?.Mode ?? string.Empty) {
                    case "chatConfig":
                        ProcessChatConfig(message);
                        break;
                    case "auth":
                        ProcessAuth(message);
                        break;
                    case "chat":
                        ProcessChat(message);
                        break;
                }
            } catch (Exception e) {
                Log.Error("[IRC] 接收消息出错\n{message}", e.Message);
            }
        }
    }

    private static void ProcessAuth(string message)
    {
        Account.Logout();
        var messageObj = JsonSerializer.Deserialize<EntityMessage>(message);
        if (messageObj == null) {
            Log.Error("[IRC] 登录失败");
            return;
        }

        Log.Error("[IRC] 登录失败: {message}", messageObj.Message);
    }

    /// <summary>
    ///     发送认证请求
    /// </summary>
    public static async Task AuthenticateAsync()
    {
        if (_webSocket.State != WebSocketState.Open) {
            return;
        }

        if (Account.IsNullOrEmpty()) {
            return;
        }

        var authMessage = new {
            mode = "auth",
            account = Account.Account,
            token = Account.Token
        };

        await SendAsync(authMessage);

        Log.Information("已发送认证请求。账户: {Account}", Account.Account);

        await SetFriendlyAsync(_friendlyMode);
    }

    public static void SetFriendly(string value)
    {
        _ = SetFriendlyAsync("true".Equals(value));
    }

    public static void SetFriendly(bool value)
    {
        _ = SetFriendlyAsync(value);
    }

    private static async Task SetFriendlyAsync(bool value)
    {
        _friendlyMode = value;

        if (_webSocket.State != WebSocketState.Open) {
            return;
        }

        if (Account.IsNullOrEmpty()) {
            return;
        }

        var friendlyMessage = new {
            mode = "friendlyMode", value
        };

        Log.Information("友好模式状态: {Friendly}", value);

        // 做事要讲良心
        await SendAsync(friendlyMessage);
    }

    private static async Task RefreshChatConfigAsync()
    {
        if (_webSocket.State != WebSocketState.Open) {
            return;
        }

        var authMessage = new {
            mode = "refresh"
        };

        await SendAsync(authMessage);
    }

    private static void ProcessChat(string text)
    {
        var message = JsonSerializer.Deserialize<EntityText>(text);
        if (message == null) {
            Log.Error("[IRC] 解析聊天失败");
            return;
        }

        Log.Information("[IRC] 收到聊天消息: {message}", message.Text);
        SendGameMessage(message.Text);
    }

    private static void ProcessChatConfig(string text)
    {
        var message = JsonSerializer.Deserialize<EntityChatConfig>(text);
        if (message == null) {
            Log.Error("[IRC] 解析聊天配置失败");
            return;
        }

        Config = message;
    }

    private static void SendGameMessage(string message)
    {
        foreach (var gameConnection in ChatManager.List) {
            PacketTools.SendGameMessage(message, gameConnection);
        }
    }

    public static void SendMessage(string message)
    {
        SendMessageAsync(message).Wait();
    }

    private static async Task SendMessageAsync(string message)
    {
        var entityChat = new EntityChat {
            Message = message
        };
        await SendAsync(entityChat);
    }

    private static async Task SendAsync<TValue>(TValue value)
    {
        if (_webSocket.State != WebSocketState.Open) {
            return;
        }

        var jsonMsg = JsonSerializer.Serialize(value);
        var buffer = Encoding.UTF8.GetBytes(jsonMsg);
        await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public static Dictionary<EntityChatPlayer, EntityChatJoin> GetPlayers(string gameId)
    {
        var players = new Dictionary<EntityChatPlayer, EntityChatJoin>();
        foreach (var chatPlayer in Config.Players) {
            foreach (var player in chatPlayer.Players) {
                if (player.Equals(gameId)) {
                    players.Add(chatPlayer, player);
                }
            }
        }

        return players;
    }

    public static (EntityChatPlayer, EntityChatJoin)? GetPlayer(string gameId, int entityId)
    {
        foreach (var chatPlayer in GetPlayers(gameId)) {
            foreach (var player in chatPlayer.Key.Players) {
                if (entityId.Equals(player.EntityId)) {
                    return (chatPlayer.Key, player);
                }
            }
        }

        return null;
    }
}