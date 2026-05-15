using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Nirvana.Development.Analysis;
using Nirvana.Development.Handlers;
using Nirvana.Development.Manager;
using Nirvana.DevPlugin;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Events.Event;
using Nirvana.DevPlugin.Extensions;
using Serilog;

namespace Nirvana.Development.Connection;

public class GameConnection : BGameConnection {
    private MultithreadEventLoopGroup? _workerGroup;

    public void Prepare()
    {
        if (_workerGroup != null) {
            Shutdown();
        }

        _workerGroup = new MultithreadEventLoopGroup();
        var bootstrap = new Bootstrap();
        bootstrap.Group(_workerGroup);
        bootstrap.Channel<TcpSocketChannel>();
        bootstrap.Option(ChannelOption.TcpNodelay, true); // 禁用Nagle算法
        bootstrap.Option(ChannelOption.SoKeepalive, true); // 保持连接
        bootstrap.Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default);
        bootstrap.Option(ChannelOption.SoSndbuf, 1048576); // 发送缓冲区大小
        bootstrap.Option(ChannelOption.SoRcvbuf, 1048576); // 接收缓冲区大小
        bootstrap.Option(ChannelOption.WriteBufferHighWaterMark, 1048576); // 发送缓冲区水水位
        bootstrap.Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(30.0)); // 连接超时时间
        bootstrap.Handler(new ActionChannelInitializer<IChannel>(channel => {
            channel.Pipeline.AddLast("splitter", new MessageDeserializer21Bit());
            channel.Pipeline.AddLast("handler", new ClientHandler(this));
            channel.Pipeline.AddLast("pre-encoder", new MessageSerializer21Bit());
            channel.Pipeline.AddLast("encoder", new MessageSerializer());
        }));
        var initialized = false;
        // Config.ForwardAddress = "localhost";
        // Config.ForwardPort = 25577;
        Task.Run(() => {
            EventManager.TriggerEvent<IEventParseAddress>(eventAddress => { eventAddress.OnParseAddress(Config); }, ProtocolVersion);
            var serverChannel = (IPAddress.TryParse(Config.ForwardAddress, out var address) ? bootstrap.ConnectAsync(address, Config.ForwardPort) : bootstrap.ConnectAsync(Config.ForwardAddress, Config.ForwardPort)).ContinueWith(channel => {
                if (!channel.IsFaulted) {
                    return channel.GetAwaiter().GetResult();
                }

                Log.Error(channel.Exception, "Failed to connect to remote server {0}:{1}", Config.ForwardAddress, Config.ForwardPort);
                return null;
            });
            ServerChannel = serverChannel.GetAwaiter().GetResult();
            initialized = true;
        });
        while (!initialized) {
            Thread.Sleep(100);
        }

        if (ServerChannel == null) {
            Shutdown();
        }
    }

    public void OnServerReceived(IByteBuffer buffer)
    {
        HandlePacketReceived(buffer, EnumPacketDirection.ClientBound, data => { ClientChannel?.WriteAndFlushAsync(data); });
    }

    public void OnClientReceived(IByteBuffer buffer)
    {
        HandlePacketReceived(buffer, EnumPacketDirection.ServerBound, data => { ServerChannel?.WriteAndFlushAsync(data); });
    }

    public void Shutdown()
    {
        EventManager.TriggerEvent<IEventConnectionClosed>(eventClosed => { eventClosed.OnConnectionClosed(); }, ProtocolVersion);
        Log.Debug("Shutting down connection...");
        ClientChannel?.CloseAsync();
        ServerChannel?.CloseAsync();
        _workerGroup?.ShutdownGracefullyAsync();
    }

    private void HandlePacketReceived(IByteBuffer buffer, EnumPacketDirection direction, Action<object> onRedirect)
    {
        buffer.MarkReaderIndex();
        var id = buffer.ReadVarIntFromBuffer();

        var packet = PacketManager.TriggerEvent(iPacket => {
            try {
                // Log.Information("Handle[{0}.{1}]: {2}[{3}]", direction, State, iPacket.GetType().Name, id);
                iPacket.PacketId = id;
                iPacket.ProtocolVersion = ProtocolVersion;
                iPacket.ReadFromBuffer(this, buffer);
            } catch (Exception exception) {
                Log.Error(exception, "Cannot read packet from buffer, direction: {0}, Id: {1}, ProtocolVersion: {2}", direction, id, ProtocolVersion);
                throw;
            }

            try {
                if (iPacket.HandlePacket(this)) {
                    return true;
                }
            } catch (Exception exception) {
                Log.Error(exception, "Cannot handle packet, direction: {0}, Id: {1}, ProtocolVersion: {2}", direction, id, ProtocolVersion);
                throw;
            }

            try {
                buffer.Clear();
                buffer.WriteVarInt(id);
                iPacket.WriteToBuffer(buffer);
                buffer.MarkReaderIndex();
                buffer.ReadVarIntFromBuffer();
            } catch (Exception exception) {
                Log.Error(exception, "Cannot write packet to buffer, direction: {0}, Id: {1}, ProtocolVersion: {2}", direction, id, ProtocolVersion);
                throw;
            }

            return false;
        }, State, direction, id, ProtocolVersion, Config.GameId);

        if (packet == null) {
            // Log.Information("Handle[{0}.{1}]: {2}", direction, State, id);
            buffer.ResetReaderIndex();
            onRedirect(buffer);
        }
    }

    public static void EnableCompression(IChannel channel, int threshold)
    {
        if (threshold < 0) {
            if (channel.Pipeline.Get("decompress") is NettyCompressionDecoder) {
                channel.Pipeline.Remove("decompress");
            }

            if (channel.Pipeline.Get("compress") is NettyCompressionEncoder) {
                channel.Pipeline.Remove("compress");
            }
        } else {
            if (channel.Pipeline.Get("decompress") is NettyCompressionDecoder nettyCompressionDecoder) {
                nettyCompressionDecoder.Threshold = threshold;
            } else {
                channel.Pipeline.AddAfter("splitter", "decompress", new NettyCompressionDecoder(threshold));
            }

            if (channel.Pipeline.Get("compress") is NettyCompressionEncoder nettyCompressionEncoder) {
                nettyCompressionEncoder.Threshold = threshold;
            } else {
                channel.Pipeline.AddBefore("encoder", "compress", new NettyCompressionEncoder(threshold));
            }
        }
    }

    public static void EnableEncryption(IChannel channel, byte[] secretKey)
    {
        channel.Pipeline.AddBefore("splitter", "decrypt", new NettyEncryptionDecoder(secretKey));
        channel.Pipeline.AddBefore("pre-encoder", "encrypt", new NettyEncryptionEncoder(secretKey));
    }
}