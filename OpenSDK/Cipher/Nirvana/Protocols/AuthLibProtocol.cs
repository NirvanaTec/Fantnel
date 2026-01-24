using System.Net;
using System.Net.Sockets;
using System.Text;
using OpenSDK.Cipher.Nirvana.Connection;
using Serilog;

namespace OpenSDK.Cipher.Nirvana.Protocols;

public class AuthLibProtocol(IPAddress address, int port, string modList, string version, string accessToken)
    : IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    private Task? _acceptLoopTask;

    private bool _disposed;

    private TcpListener? _listener;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~AuthLibProtocol()
    {
        Dispose(false);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _cts.Cancel();
            _listener?.Stop();
            try
            {
                _acceptLoopTask?.Wait(TimeSpan.FromSeconds(5L));
            }
            catch (Exception ex)
            {
                Log.Error("Authentication failed. {Message}", ex.Message);
            }

            _cts.Dispose();
        }

        _disposed = true;
    }

    public void Start()
    {
        if (!_disposed)
        {
            _listener = new TcpListener(address, port);
            _listener.Start();
            _acceptLoopTask = AcceptLoopAsync(_cts.Token);
        }
        else
        {
            throw new ObjectDisposedException("AuthLibProtocol");
        }
    }

    public void Stop()
    {
        if (!_disposed) Dispose();
    }

    private async Task AcceptLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested && !_disposed)
            try
            {
                if (_listener != null)
                    await HandleClientAsync(await _listener.AcceptTcpClientAsync(token).ConfigureAwait(false), token);
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex2)
            {
                Log.Warning("Accept loop error: {Message}", ex2.Message);
                break;
            }
    }

    private static async Task ReadExactAsync(NetworkStream stream, byte[] buffer, int offset, int count,
        CancellationToken token)
    {
        int num;
        for (var read = 0; read < count; read += num)
        {
            num = await stream.ReadAsync(buffer.AsMemory(offset + read, count - read), token).ConfigureAwait(false);
            if (num == 0) throw new EndOfStreamException();
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        using (client)
        {
            await using var stream = client.GetStream();
            var responseCode = 1u;
            try
            {
                var lenBuf = new byte[4];

                await ReadExactAsync(stream, lenBuf, 0, 4, token).ConfigureAwait(false);
                var gameIdLen = BitConverter.ToInt32(lenBuf);
                var gameIdBuf = new byte[gameIdLen];
                await ReadExactAsync(stream, gameIdBuf, 0, gameIdLen, token).ConfigureAwait(false);
                var gameId = Encoding.UTF8.GetString(gameIdBuf);

                await ReadExactAsync(stream, lenBuf, 0, 4, token).ConfigureAwait(false);
                var userIdLen = BitConverter.ToInt32(lenBuf);
                var userIdBuf = new byte[userIdLen];
                await ReadExactAsync(stream, userIdBuf, 0, userIdLen, token).ConfigureAwait(false);
                var userId = Encoding.UTF8.GetString(userIdBuf);

                await ReadExactAsync(stream, lenBuf, 0, 4, token).ConfigureAwait(false);
                var serverIdLen = BitConverter.ToInt32(lenBuf);
                var serverIdBuf = new byte[serverIdLen];
                await ReadExactAsync(stream, serverIdBuf, 0, serverIdLen, token).ConfigureAwait(false);
                var serverId = Encoding.UTF8.GetString(serverIdBuf);

                await NetEaseConnection.CreateAuthenticatorAsync(serverId, gameId, version, modList,
                    int.Parse(userId), accessToken, () => { responseCode = 0u; }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Warning("Client handling error: {Message}", ex.Message);
            }
            finally
            {
                try
                {
                    var bytes = BitConverter.GetBytes(responseCode);
                    await stream.WriteAsync(bytes, token).ConfigureAwait(false);
                }
                catch (Exception ex2)
                {
                    Log.Warning("Response writing error: {Message}", ex2.Message);
                }
            }
        }
    }
}