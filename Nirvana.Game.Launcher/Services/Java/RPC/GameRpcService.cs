using System.Net;
using System.Net.Sockets;
using Codexus.Development.SDK.Manager;
using Codexus.Game.Launcher.Managers;
using Codexus.Game.Launcher.Services.Java.RPC.Events;
using Codexus.Game.Launcher.Utils;
using Nirvana.Game.Launcher.Entities;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.Launch.RPC;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.Launch.Skin;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.Minecraft;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.Texture;
using Nirvana.WPFLauncher.Protocol;
using Nirvana.WPFLauncher.Utils;
using OpenSDK.Cipher.Nirvana;
using Serilog;

namespace Nirvana.Game.Launcher.Services.Java.RPC;

public class GameRpcService(int port, EntityLaunchGame launchGame, EnumGameVersion gameVersion) {
    private readonly HttpClient _httpClient = new();

    private readonly SocketCallback _mSocketCallbackFuc = new();

    private readonly List<byte[]> _sendCache = [];

    private readonly Skip32Cipher _skip32Cipher = new("SaintSteve"u8.ToArray());

    private string _dirSkinPath = string.Empty;

    private bool _mIsLaunchIdxReady;

    private bool _mIsNormalExit;

    private TcpListener? _mMcControlListener;

    private BinaryReader? _reader;

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
            _ = Task.Run(ListenControlConnect);
            Console.WriteLine();
            Log.Information("[RPC] Control connection started on port {0}", port);
        } catch (Exception exception) {
            if (tryTimes > 0) {
                StartControlConnection(tryTimes - 1);
            } else {
                CloseControlConnection();
            }

            Console.WriteLine();
            Log.Error(exception, "[RPC] Failed to start control connection after retries");
        }
    }

    public void CloseControlConnection()
    {
        _mIsNormalExit = true;
        _mMcControlListener?.Stop();
        _mMcControlListener = null;
        CloseGameCleaning();
        Log.Information("[RPC] Control connection closed");
    }

    private void HandleAuthenticationNewVersion(byte[] data)
    {
        if (gameVersion > EnumGameVersion.V_1_18) {
            var array = SimplePack.Pack((ushort)1799, launchGame.ServerIp, launchGame.ServerPort, launchGame.RoleName);
            if (array != null) {
                SendControlData(array);
            }

            Log.Information("[RPC] Sent new-version authentication to {0}:{1} | User: {2} | Role: {3} | Protocol: {4}", launchGame.ServerIp, launchGame.ServerPort, launchGame.Account.UserId, launchGame.RoleName, gameVersion);
        }
    }

    private void HandleAuthentication(byte[] data)
    {
        var array = SimplePack.Pack((ushort)1031, launchGame.ServerIp, launchGame.ServerPort, launchGame.RoleName, false);
        if (array != null) {
            SendControlData(array);
        }

        Log.Information("[RPC] Sent authentication to {0}:{1} | User: {2} | Role: {3} | Protocol: {4}", launchGame.ServerIp, launchGame.ServerPort, launchGame.Account.UserId, launchGame.RoleName, gameVersion);
    }

    private void OnHeartBeat(byte[] data)
    {
        var array = SimplePack.Pack((ushort)512, "i'am wpflauncher");
        if (array != null) {
            SendControlData(array);
        }

        Log.Information("[RPC] Heartbeat {0} sent", "i'am wpflauncher");
    }

    private static void OnPCycEntityCheck(byte[] data)
    {
        Log.Information("[RPC] PCyc Entity {0} sent", "[]");
    }

    private void HandlePlayerSkin(byte[] content)
    {
        Log.Information("[RPC] Event received -> {0}", "Send Player Skin");
        var entityOtherEnterWorldMsg = new EntityOtherEnterWorldMsg();
        new SimpleUnpack(content).Unpack(ref entityOtherEnterWorldMsg);
        TaskManager.Instance.GetFactory().StartNew(() => ProcessPlayerSkin(entityOtherEnterWorldMsg));
    }

    private async Task ProcessPlayerSkin(EntityOtherEnterWorldMsg msg)
    {
        var list = await NPFLauncher.GetSkinListInGameAsync(launchGame.Account.GetUserId(), launchGame.Account.GetToken(), new EntityUserGameTextureRequest {
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
                var entityAvailableUser = IUserManager.Instance?.GetAvailableUser(launchGame.Account.GetUserId());
                if (entityAvailableUser == null) {
                    continue;
                }

                var entity = await NPFLauncher.GetNetGameComponentDownloadListAsync(launchGame.Account.GetUserId(), launchGame.Account.GetToken(), item.SkinId);
                var text = entity.SubEntities.Select(sub => sub.ResUrl).FirstOrDefault();
                if (text != null) {
                    Directory.CreateDirectory(_dirSkinPath);
                    await FileUtil.WriteFileSafelyAsync(tempPath, await _httpClient.GetByteArrayAsync(text));
                    if (File.Exists(tempPath) && FileUtil.IsFileReadable(tempPath)) {
                        filePath = tempPath;
                        break;
                    }
                }
            } catch (Exception exception) {
                Log.Error(exception, "[RPC] Failed to handle skin for player {Name}", msg.Name);
                try {
                    if (File.Exists(tempPath)) {
                        File.Delete(tempPath);
                    }
                } catch {
                    Log.Error(exception, "[RPC] Failed to delete temp file {0}", filePath);
                }
            }
        }

        Log.Information("[RPC] Sending skin data for {0}: {1}", msg.Name, filePath);
        var array = SimplePack.Pack((ushort)520, msg.Name, filePath, string.Empty, skinMode);
        if (array != null) {
            SendControlData(array);
        }
    }

    private static void HandleLoginGame(byte[] data)
    {
        Log.Information("[RPC] Event received -> {0}", "Login Game");
    }

    private void HandleMsgFilterCheck(byte[] data)
    {
        var array = SimplePack.Pack((ushort)1298, false, 0L, 0L);
        if (array != null) {
            SendControlData(array);
        }

        Log.Information("[RPC] Event received -> {0}", "Filter Message Check");
    }

    private void OnCheckPlayerMsg(byte[] data)
    {
        EntityCheckPlayerMessage? content = null;
        new SimpleUnpack(data).Unpack(ref content);
        if (content != null) {
            var array = SimplePack.Pack((ushort)18, content.Length, content.Message);
            if (array != null) {
                SendControlData(array);
            }
        }

        Log.Information("[RPC] Event received -> {0} with data {1}", "Player Message Check", content?.Message);
    }

    private void ListenControlConnect()
    {
        try {
            while (!_mIsNormalExit) {
                var tcpClient = _mMcControlListener?.AcceptTcpClient();
                if (tcpClient == null) {
                    continue;
                }

                var stream = tcpClient.GetStream();
                _reader = new BinaryReader(stream);
                _writer = new BinaryWriter(stream);
                SendCacheControlData();
                _ = Task.Run(OnRecvControlData);
                Log.Information("[RPC] Accepted control connection from {0}", tcpClient.Client.RemoteEndPoint?.ToString());
            }
        } catch {
            Log.Error("[RPC] Failed to listen control connection");
        }
    }

    private void SendControlData(byte[] message)
    {
        if (_writer == null) {
            _sendCache.Add(message);
            return;
        }

        var buffer = BitConverter.GetBytes((ushort)message.Length).Concat(message).ToArray();
        try {
            _writer?.Write(buffer);
            _writer?.Flush();
        } catch (Exception exception) {
            Log.Error(exception, "[RPC] Failed to send control data");
        }
    }

    private void OnRecvControlData()
    {
        while (!_mIsNormalExit) {
            try {
                if (_reader != null) {
                    var count = _reader.ReadUInt16();
                    var message = _reader.ReadBytes(count);
                    HandleMcControlMessage(message);
                }
            } catch (EndOfStreamException) {
                break;
            } catch (IOException) {
                break;
            } catch (Exception exception) {
                Log.Error(exception, "[RPC] Error receiving control data");
                if (!_mIsNormalExit) {
                    CloseGameCleaning();
                }

                break;
            }
        }
    }

    private void HandleMcControlMessage(byte[] message)
    {
        var num = BitConverter.ToUInt16(message, 0);
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
        foreach (var item in _sendCache) {
            SendControlData(item);
        }

        _sendCache.Clear();
    }
}