using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Nirvana.DevPlugin.Entities;
using Nirvana.Game.Launcher.Utils;
using Nirvana.WPFLauncher.Utils;
using Serilog;

namespace Nirvana.Development.Utils;

public class UdpBroadcaster {
    private readonly InterceptorConfig _config;

    private readonly int _serverPort;

    private readonly IPEndPoint _targetEndPoint;

    private readonly UdpClient _udpClient;

    private bool _running;

    public UdpBroadcaster(int targetPort, InterceptorConfig config, string multicastAddress = "224.0.2.60", int port = 4445)
    {
        _udpClient = new UdpClient();
        _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _targetEndPoint = new IPEndPoint(IPAddress.Parse(multicastAddress), port);
        _udpClient.MulticastLoopback = true;
        _serverPort = targetPort;
        _config = config;
    }


    public async Task StartBroadcastingAsync()
    {
        _running = true;
        try {
            while (_running) {
                await SendMessageAsync();
                await Task.Delay(2000);
            }
        } catch (OperationCanceledException ex) {
            Log.Error("Broadcasting operation cancelled, {0}", ex.Message);
        } catch (Exception value) {
            Log.Error("UDP Broadcast error: {0}", value);
        }
    }

    private async Task SendMessageAsync()
    {
        try {
            var message = BuildMessage();
            var bytes = Encoding.UTF8.GetBytes(message);
            await _udpClient.SendAsync(bytes, bytes.Length, _targetEndPoint);
        } catch (SocketException ex) when (ex.SocketErrorCode == SocketError.HostUnreachable) {
            await Task.Delay(5000);
        } catch (Exception value) {
            Log.Error("UDP Send failed: {0}", value);
        }
    }

    private string BuildMessage()
    {
        var gameVersion = GameVersionUtil.GetEnumFromGameVersion(_config.ServerVersion);
        return gameVersion > EnumGameVersion.V_1_8_9 ? $"[MOTD] §cNirvana §f{_config.ServerName} -> {_config.NickName}[/MOTD][AD]{_serverPort}[/AD]" : $"[MOTD] Nirvana {_config.ServerName} -> {_config.NickName}[/MOTD][AD]{_serverPort}[/AD]";
    }

    public void Stop()
    {
        _running = false;
    }

}