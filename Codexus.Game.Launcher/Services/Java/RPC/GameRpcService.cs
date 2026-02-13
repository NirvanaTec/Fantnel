using System.Net;
using System.Net.Sockets;
using Codexus.Development.SDK.Manager;
using Codexus.Game.Launcher.Entities;
using Codexus.Game.Launcher.Managers;
using Codexus.Game.Launcher.Services.Java.RPC.Events;
using NirvanaAPI.Utils;
using OpenSDK.Cipher.Nirvana;
using Serilog;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.Launch.RPC;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.Launch.Skin;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.Minecraft;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.Texture;
using WPFLauncherApi.Protocol;
using WPFLauncherApi.Utils;

namespace Codexus.Game.Launcher.Services.Java.RPC;

public class GameRpcService(
    int port,
    EntityLaunchGame launchGame,
    EnumGameVersion gameVersion) {
    private readonly HttpClient _httpClient = new();
    private readonly Lock _lockObj = new();
    private readonly SocketCallback _mSocketCallbackFuc = new();
    private readonly List<byte[]> _sendCache = [];
    private readonly Skip32Cipher _skip32Cipher = new("SaintSteve"u8.ToArray());

    private TcpClient? _client;

    private string _dirSkinPath = string.Empty;
    private bool _mIsLaunchIdxReady;
    private bool _mIsNormalExit;
    private TcpListener? _mMcControlListener;
    private BinaryReader? _reader;
    private NetworkStream? _stream;
    private BinaryWriter? _writer;

    public void Connect(string skinPath)
    {
        _dirSkinPath = skinPath;
        StartControlConnection(2);
        _mSocketCallbackFuc.RegisterReceiveCallback(18, OnCheckPlayerMsg);
        _mSocketCallbackFuc.RegisterReceiveCallback(19, OnPCycEntityCheck);
        _mSocketCallbackFuc.RegisterReceiveCallback(261, HandleAuthenticationNewVersion);
        _mSocketCallbackFuc.RegisterReceiveCallback(512, OnHeartBeat);
        _mSocketCallbackFuc.RegisterReceiveCallback(517, HandleAuthentication);
        _mSocketCallbackFuc.RegisterReceiveCallback(520, HandlePlayerSkin);
        _mSocketCallbackFuc.RegisterReceiveCallback(1298, HandleMsgFilterCheck);
        _mSocketCallbackFuc.RegisterReceiveCallback(4612, HandleLoginGame);
    }

    private void StartControlConnection(int tryTimes)
    {
        try {
            _mMcControlListener = new TcpListener(IPAddress.Loopback, port);
            _mMcControlListener.Start();
            new Thread(ListenControlConnect).Start();
            Console.WriteLine();
            Log.Information("[RPC] Control connection started on port {Port}", port);
        } catch (Exception ex) {
            if (tryTimes > 0)
                StartControlConnection(tryTimes - 1);
            else
                CloseControlConnection();
            Console.WriteLine();
            Log.Error(ex, "[RPC] Failed to start control connection after retries");
        }
    }

    public void CloseControlConnection()
    {
        _mIsNormalExit = true;

        // 关闭监听器
        try {
            _mMcControlListener?.Stop();
        } catch {
            // ignored
        }

        _mMcControlListener = null;

        // 清理连接资源
        lock (_lockObj) {
            try {
                _reader?.Dispose();
            } catch {
                // ignored
            }

            try {
                _writer?.Dispose();
            } catch {
                // ignored
            }

            try {
                _stream?.Dispose();
            } catch {
                // ignored
            }

            try {
                _client?.Close();
            } catch {
                // ignored
            }

            _reader = null;
            _writer = null;
            _stream = null;
            _client = null;
        }

        CloseGameCleaning();
        Log.Information("[RPC] Control connection closed");
    }

    private void HandleAuthenticationNewVersion(byte[] data)
    {
        if (gameVersion <= EnumGameVersion.V_1_18) return;
        var array = SimplePack.Pack((ushort)1799, launchGame.ServerIp, launchGame.ServerPort, launchGame.RoleName);
        if (array != null) SendControlData(array);
        Log.Information(
            "[RPC] Sent new-version authentication to {ServerIP}:{ServerPort} | User: {UserID} | Role: {RoleName} | Protocol: {GameVersion}",
            launchGame.ServerIp, launchGame.ServerPort, launchGame.UserId, launchGame.RoleName, gameVersion);
    }

    private void HandleAuthentication(byte[] data)
    {
        var array = SimplePack.Pack((ushort)1031, launchGame.ServerIp, launchGame.ServerPort, launchGame.RoleName,
            false);
        if (array != null) SendControlData(array);
        Log.Information(
            "[RPC] Sent authentication to {ServerIP}:{ServerPort} | User: {UserID} | Role: {RoleName} | Protocol: {GameVersion}",
            launchGame.ServerIp, launchGame.ServerPort, launchGame.UserId, launchGame.RoleName, gameVersion);
    }

    private void OnHeartBeat(byte[] data)
    {
        var array = SimplePack.Pack((ushort)512, "i'am wpflauncher");
        if (array != null) SendControlData(array);
        Log.Information("[RPC] Heartbeat {heartBeatMsg} sent", "i'am wpflauncher");
    }

    private static void OnPCycEntityCheck(byte[] data)
    {
        Log.Information("[RPC] PCyc Entity {entity} sent", "[]");
    }

    private void HandlePlayerSkin(byte[] content)
    {
        Log.Information("[RPC] Event received -> {event}", "Send Player Skin");
        var entityOtherEnterWorldMsg = new EntityOtherEnterWorldMsg();
        new SimpleUnpack(content).Unpack(ref entityOtherEnterWorldMsg);
        TaskManager.Instance.GetFactory().StartNew(() => ProcessPlayerSkin(entityOtherEnterWorldMsg));
    }

    private async Task ProcessPlayerSkin(EntityOtherEnterWorldMsg msg)
    {
        var list = await WPFLauncher.GetSkinListInGameAsync(launchGame.UserId, launchGame.AccessToken,
            new EntityUserGameTextureRequest {
                UserId = _skip32Cipher.ComputeUserIdFromUuid(msg.Uuid).ToString(),
                ClientType = EnumGameClientType.Java
            });
        var filePath = string.Empty;
        var skinMode = EnumSkinMode.Default;
        foreach (var item in list.Where(s => s.SkinId.Length > 5)) {
            skinMode = (EnumSkinMode)item.SkinMode;
            var tempPath = Path.Combine(_dirSkinPath, "skin_" + item.SkinId + ".png");
            if (File.Exists(tempPath) && FileUtil.IsFileReadable(tempPath)) {
                filePath = tempPath;
                break;
            }

            try {
                var entityAvailableUser = IUserManager.Instance?.GetAvailableUser(launchGame.UserId);
                if (entityAvailableUser == null) continue;
                var entity =
                    await WPFLauncher.GetNetGameComponentDownloadListAsync(launchGame.UserId, launchGame.AccessToken,
                        item.SkinId);
                var text = entity.SubEntities.Select(sub => sub.ResUrl).FirstOrDefault();
                if (text == null) continue;
                Directory.CreateDirectory(_dirSkinPath);
                await FileUtil.WriteFileSafelyAsync(tempPath, await _httpClient.GetByteArrayAsync(text));
                if (!File.Exists(tempPath) || !FileUtil.IsFileReadable(tempPath)) continue;
                filePath = tempPath;
                break;
            } catch (Exception ex) {
                Log.Error(ex, "[RPC] Failed to handle skin for player {Name}", msg.Name);
                try {
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                } catch {
                    Log.Error(ex, "[RPC] Failed to delete temp file {Path}", filePath);
                }
            }
        }

        Log.Information("[RPC] Sending skin data for {Name}: {Path}", msg.Name, filePath);
        var array = SimplePack.Pack((ushort)520, msg.Name, filePath, string.Empty, skinMode);
        if (array != null) SendControlData(array);
    }

    private static void HandleLoginGame(byte[] data)
    {
        Log.Information("[RPC] Event received -> {event}", "Login Game");
    }

    private void HandleMsgFilterCheck(byte[] data)
    {
        var array = SimplePack.Pack((ushort)1298, false, 0L, 0L);
        if (array != null) SendControlData(array);
        Log.Information("[RPC] Event received -> {event}", "Filter Message Check");
    }

    private void OnCheckPlayerMsg(byte[] data)
    {
        EntityCheckPlayerMessage? content = null;
        new SimpleUnpack(data).Unpack(ref content);
        if (content != null) {
            var array = SimplePack.Pack((ushort)18, content.Length, content.Message);
            if (array != null) SendControlData(array);
        }

        Log.Information("[RPC] Event received -> {event} with data {Message}", "Player Message Check",
            content?.Message);
    }

    private void ListenControlConnect()
    {
        try {
            while (!_mIsNormalExit) {
                var tcpClient = _mMcControlListener?.AcceptTcpClient();
                if (tcpClient == null) continue;

                // 验证连接来源
                var remoteEndPoint = tcpClient.Client.RemoteEndPoint as IPEndPoint;
                if (remoteEndPoint?.Address.ToString() != IPAddress.Loopback.ToString()) {
                    try {
                        tcpClient.Close();
                    } catch {
                        // ignored
                    }

                    continue;
                }

                Log.Information("[RPC] Accepted control connection from {RemoteEndPoint}", remoteEndPoint);

                // 清理旧连接并保存新连接
                lock (_lockObj) {
                    // 关闭旧连接
                    try {
                        _reader?.Dispose();
                    } catch {
                        // ignored
                    }

                    try {
                        _writer?.Dispose();
                    } catch {
                        // ignored
                    }

                    try {
                        _stream?.Dispose();
                    } catch {
                        // ignored
                    }

                    try {
                        _client?.Close();
                    } catch {
                        // ignored
                    }

                    // 保存新连接
                    _client = tcpClient;
                    _stream = tcpClient.GetStream();
                    _reader = new BinaryReader(_stream);
                    _writer = new BinaryWriter(_stream);
                }

                // 发送缓存数据
                SendCacheControlData();

                // 接收数据
                new Thread(OnRecvControlData).Start();
            }
        } catch (Exception ex) {
            if (!_mIsNormalExit) // 忽略正常退出时的异常
            {
                Log.Error(ex, "[RPC] Failed to listen control connection");
                CloseControlConnection();
            }
        }
    }

    private void SendControlData(byte[] message)
    {
        lock (_lockObj) {
            if (_writer == null) {
                _sendCache.Add(message);
                return;
            }

            try {
                var buffer = BitConverter.GetBytes((ushort)message.Length).Concat(message).ToArray();
                _writer.Write(buffer);
                _writer.Flush();
            } catch (Exception ex) {
                Log.Error(ex, "[RPC] Failed to send control data");
                // 连接可能已关闭，清理资源
                try {
                    _reader?.Dispose();
                } catch {
                    // ignored
                }

                try {
                    _writer?.Dispose();
                } catch {
                    // ignored
                }

                try {
                    _stream?.Dispose();
                } catch {
                    // ignored
                }

                try {
                    _client?.Close();
                } catch {
                    // ignored
                }

                _reader = null;
                _writer = null;
                _stream = null;
                _client = null;

                // 将消息添加到缓存，等待重新连接
                _sendCache.Add(message);
            }
        }
    }

    private void OnRecvControlData()
    {
        while (!_mIsNormalExit)
            try {
                BinaryReader? currentReader;
                lock (_lockObj) {
                    currentReader = _reader;
                }

                if (currentReader == null) {
                    Thread.Sleep(100); // 避免忙等待
                    continue;
                }

                // 读取数据（注意：不要在锁内执行阻塞操作）
                var count = currentReader.ReadUInt16();
                var message = currentReader.ReadBytes(count);

                // 处理消息
                HandleMcControlMessage(message);
            } catch (EndOfStreamException) {
                Log.Information("[RPC] Connection closed by remote host");
                break;
            } catch (IOException) {
                Log.Information("[RPC] Network error occurred");
                break;
            } catch (ObjectDisposedException) {
                Log.Information("[RPC] Connection disposed");
                break;
            } catch (Exception ex) {
                Log.Error(ex, "[RPC] Error receiving control data");
                if (!_mIsNormalExit) CloseControlConnection();
                break;
            }
    }

    private void HandleMcControlMessage(byte[] message)
    {
        var num = BitConverter.ToUInt16(message);
        var parameters = message.Skip(2).ToArray();
        if (!_mIsLaunchIdxReady && num == 261) {
            _mIsLaunchIdxReady = true;
            Log.Information("[RPC] Launch index ready, executed ready actions");
        }

        _mSocketCallbackFuc.InvokeCallback(num, parameters);
    }

    private void CloseGameCleaning()
    {
        _mIsLaunchIdxReady = false;
        Log.Information("[RPC] Cleaned up game resources");
    }

    private void SendCacheControlData()
    {
        if (_sendCache.Count == 0) return;

        lock (_lockObj) {
            if (_writer == null) return;

            foreach (var item in _sendCache)
                try {
                    var buffer = BitConverter.GetBytes((ushort)item.Length).Concat(item).ToArray();
                    _writer.Write(buffer);
                } catch (Exception ex) {
                    Log.Error(ex, "[RPC] Failed to send cached control data");
                    break;
                }

            try {
                _writer.Flush();
            } catch {
                // ignored
            }

            _sendCache.Clear();
        }
    }
}