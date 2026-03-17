using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Codexus.Development.SDK.Connection;
using Nirvana.Chat.Entities;
using NirvanaAPI;
using NirvanaAPI.Entities.EntitiesNirvana;
using Serilog;

namespace Nirvana.Chat.Message;

public static class ChatMessage {
    
    private static readonly Uri Url = new("ws://110.42.70.32:13423/ws/chat");
    private static EntityChatConfig _config = new();

    private static Timer? _heartbeat; // 心跳定时器
    private static ClientWebSocket _webSocket = new();

    private static readonly List<GameConnection> GameConnections = [];

    public static void Start(GameConnection gameConnection)
    {
        _ = StartAsync(gameConnection);
    }

    public static async Task StartAsync()
    {
        foreach (var gameConnection in GameConnections) {
            await StartAsync(gameConnection);
        }
    }

    private static async Task Join(GameConnection gameConnection)
    {
        if (GameConnections.Any(a => a.GameId == gameConnection.GameId && a.NickName == gameConnection.NickName)) {
            return;
        }
        var entity = new EntityChatJoin {
            GameId = gameConnection.GameId,
            NickName = gameConnection.NickName
        };
        GameConnections.Add(gameConnection);
        await SendAsync(entity);
    }

    public static async Task RemoveJoin(GameConnection gameConnection)
    {
        foreach (var a in GameConnections.Where(a => a.GameId == gameConnection.GameId && a.NickName == gameConnection.NickName)) {
            GameConnections.Remove(a);
            break;
        }
        var entity = new EntityChatJoin {
            Mode = "removeJoin",
            GameId = gameConnection.GameId,
            NickName = gameConnection.NickName
        };
        await SendAsync(entity);
    }

    private static async Task StartAsync(GameConnection gameConnection)
    {
        if (_webSocket.State != WebSocketState.Open) {
            await _webSocket.ConnectAsync(Url, CancellationToken.None);
            Log.Information("[IRC] 连接成功!");

            if (NirvanaConfig.Config.IsNullOrEmpty()) {
                await RefreshChatConfigAsync();
            } else {
                await AuthenticateAsync();
            }

            new Thread(() => Initialize(gameConnection)).Start();
        }

        if (_heartbeat == null) {
            var interval = TimeSpan.FromMinutes(5);
            _heartbeat = new Timer(OnHeartbeat, null, TimeSpan.Zero, interval);
        }

        await Join(gameConnection);
    }

    private static void OnHeartbeat(object? state)
    {
        try {
            if (_config.Heartbeats.Count == 0 || !NirvanaConfig.Config.ChatEnable) {
                return;
            }

            // 随机取 IrcInfo.Heartbeats
            var heartbeat = _config.GetHeartbeat();
            if (string.IsNullOrEmpty(heartbeat)) {
                return;
            }

            SendGameMessage(heartbeat);
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
        if (_heartbeat != null) {
            try {
                await _heartbeat.DisposeAsync(); // 停下
                _heartbeat = null;
            } catch (Exception e) {
                Log.Error("[IRC] 关闭心跳失败\n{message}", e.Message);
            }
        }

        try {
            if (_webSocket.State == WebSocketState.Open) {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing",
                    CancellationToken.None);
            }
        } catch (Exception e) {
            Log.Error("[IRC] 关闭连接失败\n{message}", e.Message);
        }

        _webSocket = new ClientWebSocket();
    }

    private static void Initialize(GameConnection gameConnection)
    {
        InitializeAsync(gameConnection).Wait();
    }

    private static async Task InitializeAsync(GameConnection gameConnection)
    {
        var buffer = new byte[1024 * 4]; // 4KB 缓冲区

        while (_webSocket.State == WebSocketState.Open) {
            try {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close) {
                    Log.Error("[IRC] 服务器要求关闭连接。");
                    Shutdown();
                    Start(gameConnection);
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
        NirvanaConfig.Config.Logout();
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

        if (NirvanaConfig.Config.IsNullOrEmpty()) {
            return;
        }

        var authMessage = new {
            mode = "auth",
            account = NirvanaConfig.Config.Account,
            token = NirvanaConfig.Config.Token
        };

        await SendAsync(authMessage);

        Log.Information("已发送认证请求。账户: {Account}", NirvanaConfig.Config.Account);
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

        _config = message;
    }

    private static void SendGameMessage(string message)
    {
        foreach (var gameConnection in GameConnections) {
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
}