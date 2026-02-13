using System.Net;
using Codexus.Development.SDK.Analysis;
using Codexus.Development.SDK.Entities;
using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Event;
using Codexus.Development.SDK.Extensions;
using Codexus.Development.SDK.Handlers;
using Codexus.Development.SDK.Manager;
using Codexus.Development.SDK.Utils;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using OpenTl.Netty.Socks.Handlers;
using Serilog;

namespace Codexus.Development.SDK.Connection;

public class GameConnection(
    EntitySocks5 socks5,
    string modInfo,
    string gameId,
    string forwardAddress,
    int forwardPort,
    string nickName,
    string userId,
    string userToken,
    IChannel channel,
    Action<string> onJoinServer) : IConnection {
    public readonly IChannel ClientChannel = channel;

    private bool _initialized;
    private MultithreadEventLoopGroup? _workerGroup;

    public IChannel? ServerChannel;

    public string NickName { get; set; } = nickName;

    public EnumProtocolVersion ProtocolVersion { get; set; } = EnumProtocolVersion.None;

    public EnumConnectionState State { get; set; }

    public Action<string> OnJoinServer { get; set; } = onJoinServer;

    public MultithreadEventLoopGroup TaskGroup { get; } = new();

    public GameSession Session { get; set; } = new(nickName, userId, userToken);

    public string GameId { get; } = gameId;

    public string ModInfo { get; } = modInfo;

    public int ForwardPort { get; } = forwardPort;

    public string ForwardAddress { get; } = forwardAddress;

    public byte[] Uuid { get; set; } = new byte[16 /*0x10*/];

    public void Prepare()
    {
        _initialized = false;
        if (_workerGroup != null) {
            Shutdown();
        }

        _workerGroup = new MultithreadEventLoopGroup();
        var bootstrap = new Bootstrap().Group(_workerGroup).Channel<TcpSocketChannel>()
            .Option(ChannelOption.TcpNodelay, true).Option(ChannelOption.SoKeepalive, true)
            .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
            .Option(ChannelOption.SoSndbuf, 1048576).Option(ChannelOption.SoRcvbuf, 1048576)
            .Option(ChannelOption.WriteBufferHighWaterMark, 1048576)
            .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(30.0)).Handler(
                new ActionChannelInitializer<IChannel>(channel => {
                    if (socks5.Enabled) {
                        if (!IPAddress.TryParse(socks5.Address, out var address))
                            address = Dns.GetHostAddressesAsync(socks5.Address).GetAwaiter().GetResult().First();
                        channel.Pipeline.AddLast("socks5",
                            new Socks5ProxyHandler(new IPEndPoint(address, socks5.Port), socks5.Username,
                                socks5.Password));
                    }

                    channel.Pipeline.AddLast("splitter", new MessageDeserializer21Bit());
                    channel.Pipeline.AddLast("handler", new ClientHandler(this))
                        .AddLast("pre-encoder", new MessageSerializer21Bit())
                        .AddLast("encoder", new MessageSerializer());
                }));
        Task.Run((Func<Task>)(async () => {
            var finalAddress = EventManager.Instance.TriggerEvent("channel_connection",
                new EventParseAddress(this, ForwardAddress, ForwardPort));
            ServerChannel =
                await (IPAddress.TryParse(finalAddress.Address, out var address)
                    ? bootstrap.ConnectAsync(address, finalAddress.Port)
                    : bootstrap.ConnectAsync(finalAddress.Address, finalAddress.Port)).ContinueWith(
                    (Func<Task<IChannel>, IChannel?>)(channel => {
                        if (!channel.IsFaulted) {
                            return channel.Result;
                        }

                        Log.Error(channel.Exception, "Failed to connect to remote server {Address}:{Port}",
                            finalAddress.Address, finalAddress.Port);
                        return null;
                    }));
            _initialized = true;
        }));
        while (!_initialized) {
            Thread.Sleep(100);
        }

        if (ServerChannel != null) {
            return;
        }

        Shutdown();
    }

    public void OnServerReceived(IByteBuffer buffer)
    {
        HandlePacketReceived(buffer, EnumPacketDirection.ClientBound,
            data => ClientChannel.WriteAndFlushAsync(data));
    }

    public void OnClientReceived(IByteBuffer buffer)
    {
        HandlePacketReceived(buffer, EnumPacketDirection.ServerBound,
            data => ServerChannel?.WriteAndFlushAsync(data));
    }

    public void Shutdown()
    {
        EventManager.Instance.TriggerEvent("channel_connection",
            new EventConnectionClosed(this));
        Log.Debug("Shutting down connection...");
        TaskGroup.ShutdownGracefullyAsync();
        ClientChannel.CloseAsync();
        ServerChannel?.CloseAsync();
        _workerGroup?.ShutdownGracefullyAsync();
    }

    private void HandlePacketReceived(
        IByteBuffer buffer,
        EnumPacketDirection direction,
        Action<object> onRedirect)
    {
        buffer.MarkReaderIndex();
        var packetId = buffer.ReadVarIntFromBuffer();
        var packets = PacketManager.Instance.BuildPacket(State, direction, ProtocolVersion, packetId);
        if (packets == null) {
            buffer.ResetReaderIndex();
            onRedirect(buffer);
            return;
        }

        foreach (var packet in packets) {
            if (packet == null) {
                continue;
            }

            var metadata = PacketManager.Instance.GetMetadata(packet);
            if (metadata is { Skip: true }) {
                continue;
            }

            packet.ClientProtocolVersion = ProtocolVersion;
            try {
                packet.ReadFromBuffer(buffer);
            } catch (Exception ex) {
                var objArray = new object[] {
                    direction,
                    packetId,
                    packet,
                    ProtocolVersion
                };
                Log.Error(ex,
                    "Cannot read packet from buffer, direction: {Direction}, Id: {Id}, Packet: {Packet}, ProtocolVersion: {ProtocolVersion}",
                    objArray);
                throw;
            }

            if (packet.HandlePacket(this)) {
                buffer.ResetReaderIndex();
                return;
            }

            buffer.Clear();
            buffer.WriteVarInt(packetId);
            buffer.ReadVarIntFromBuffer();
            packet.WriteToBuffer(buffer);
        }

        buffer.ResetReaderIndex();
        onRedirect(buffer);
    }

    public static void EnableCompression(IChannel channel, int threshold)
    {
        if (threshold < 0) {
            if (channel.Pipeline.Get("decompress") is NettyCompressionDecoder)
                channel.Pipeline.Remove("decompress");
            if (channel.Pipeline.Get("compress") is not NettyCompressionEncoder)
                return;
            channel.Pipeline.Remove("compress");
        } else {
            if (channel.Pipeline.Get("decompress") is NettyCompressionDecoder compressionDecoder)
                compressionDecoder.Threshold = threshold;
            else
                channel.Pipeline.AddAfter("splitter", "decompress", new NettyCompressionDecoder(threshold));
            if (channel.Pipeline.Get("compress") is NettyCompressionEncoder compressionEncoder)
                compressionEncoder.Threshold = threshold;
            else
                channel.Pipeline.AddBefore("encoder", "compress", new NettyCompressionEncoder(threshold));
        }
    }

    public static void EnableEncryption(IChannel channel, byte[] secretKey)
    {
        channel.Pipeline.AddBefore("splitter", "decrypt", new NettyEncryptionDecoder(secretKey))
            .AddBefore("pre-encoder", "encrypt", new NettyEncryptionEncoder(secretKey));
    }
}