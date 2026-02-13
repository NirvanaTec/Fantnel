using System.Net;
using System.Net.Sockets;
using System.Text;
using Serilog;

namespace Codexus.Development.SDK.Utils;

public class UdpBroadcaster : IDisposable {
    private readonly bool _is189Protocol;

    private readonly string _roleName;

    private readonly string _serverIp;

    private readonly int _serverPort;

    private readonly IPEndPoint _targetEndPoint;

    private readonly UdpClient _udpClient;

    private CancellationTokenSource? _cts;

    public UdpBroadcaster(string multicastAddress, int port, int targetPort, string serverIp, string roleName,
        bool is189Protocol)
    {
        _udpClient = new UdpClient();
        _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _targetEndPoint = new IPEndPoint(IPAddress.Parse(multicastAddress), port);
        if (IsMulticastAddress(_targetEndPoint.Address)) {
            _udpClient.JoinMulticastGroup(_targetEndPoint.Address);
            _udpClient.MulticastLoopback = true;
        } else {
            _udpClient.EnableBroadcast = true;
        }

        _serverPort = targetPort;
        _is189Protocol = is189Protocol;
        _serverIp = serverIp;
        _roleName = roleName;
    }

    public void Dispose()
    {
        _cts?.Dispose();
        _udpClient?.Close();
        _udpClient?.Dispose();
    }

    public async Task StartBroadcastingAsync()
    {
        _cts = new CancellationTokenSource();
        try {
            while (!_cts.IsCancellationRequested) {
                await SendMessageAsync();
                await Task.Delay(TimeSpan.FromSeconds(2L), _cts.Token);
            }
        } catch (OperationCanceledException ex) {
            Log.Error("Broadcasting operation cancelled, {exception}", ex.Message);
        } catch (Exception value) {
            Log.Error($"UDP Broadcast error: {value}");
        }
    }

    private async Task SendMessageAsync()
    {
        try {
            var s = BuildMessage();
            var bytes = Encoding.UTF8.GetBytes(s);
            await _udpClient.SendAsync(bytes, bytes.Length, _targetEndPoint);
        } catch (SocketException ex) when (ex.SocketErrorCode == SocketError.HostUnreachable) {
            if (_cts != null) {
                await Task.Delay(5000, _cts.Token);
            }
        } catch (Exception value) {
            Log.Error("UDP Send failed: {Exception}", value);
        }
    }

    private string BuildMessage()
    {
        return _is189Protocol
            ? $"[MOTD] Nirvana {_serverIp} -> {_roleName}[/MOTD][AD]{_serverPort}[/AD]"
            : $"[MOTD] §bNirvana §f{_serverIp} -> {_roleName}[/MOTD][AD]{_serverPort}[/AD]";
    }

    public void Stop()
    {
        _cts?.Cancel();
    }

    private static bool IsMulticastAddress(IPAddress address)
    {
        var addressBytes = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetwork && addressBytes[0] >= 224) {
            return addressBytes[0] <= 239;
        }

        return false;
    }
}