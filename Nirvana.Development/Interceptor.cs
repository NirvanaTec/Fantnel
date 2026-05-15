using System;
using System.Net;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Nirvana.Development.Analysis;
using Nirvana.Development.Handlers;
using Nirvana.Development.Manager;
using Nirvana.Development.Utils;
using Nirvana.DevPlugin.Entities;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Events.Event;
using NirvanaAPI.Entities.Login;
using Serilog;

namespace Nirvana.Development;

public class Interceptor {
    private IChannel? _channel;
    private UdpBroadcaster? _udpBroadcaster;

    public required MultithreadEventLoopGroup AcceptorGroup;
    public required InterceptorConfig CurrentConfig;
    public required EntityAccount CurrentUser;
    public required MultithreadEventLoopGroup WorkerGroup;

    public static Interceptor CreateInterceptor(bool isRental, string modInfo, string gameId, string serverName, string serverVersion, string forwardAddress, int forwardPort, string nickName, EntityAccount currentUser, Action<InterceptorConfig, string>? onJoinServer = null, int localPort = 25565)
    {
        var availablePort = NetworkUtil.GetAvailablePort(localPort);

        if (EventManager.TriggerEvent<IEventCreateInterceptor>(createInterceptor => createInterceptor.OnCreateInterceptor(availablePort), EnumProtocolVersion.All) != null) {
            throw new InvalidOperationException("Create Interceptor cancelled");
        }

        var parentGroup = new MultithreadEventLoopGroup();
        var childGroup = new MultithreadEventLoopGroup();
        var currentConfig = new InterceptorConfig {
            IsRental = isRental,
            LocalPort = availablePort,
            NickName = nickName,
            ForwardAddress = forwardAddress,
            ForwardPort = forwardPort <= 0 ? 25565 : forwardPort,
            ServerName = serverName,
            ServerVersion = serverVersion,
            ModInfo = modInfo,
            GameId = gameId,
            OnJoinServer = onJoinServer
        };
        var interceptor = new Interceptor {
            AcceptorGroup = parentGroup,
            WorkerGroup = childGroup,
            CurrentUser = currentUser,
            CurrentConfig = currentConfig
        };
        var serverBootstrap = new ServerBootstrap();
        serverBootstrap.Group(parentGroup, childGroup);
        serverBootstrap.Channel<TcpServerSocketChannel>();
        serverBootstrap.Option(ChannelOption.SoReuseaddr, true); // 允许地址重用
        serverBootstrap.Option(ChannelOption.SoReuseport, true); // 允许端口重用
        serverBootstrap.Option(ChannelOption.TcpNodelay, true); // 禁用Nagle算法
        serverBootstrap.Option(ChannelOption.SoKeepalive, true); // 保持连接
        serverBootstrap.Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default).Option(ChannelOption.SoSndbuf, 1048576); // 发送缓冲区大小
        serverBootstrap.Option(ChannelOption.SoRcvbuf, 1048576); // 接收缓冲区大小
        serverBootstrap.Option(ChannelOption.WriteBufferHighWaterMark, 1048576); // 发送缓冲区水水位
        serverBootstrap.Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(10.0)); // 连接超时时间
        serverBootstrap.ChildHandler(new ActionChannelInitializer<IChannel>(channel => {
            channel.Pipeline.AddLast("splitter", new MessageDeserializer21Bit());
            channel.Pipeline.AddLast("handler", new ServerHandler(currentConfig));
            channel.Pipeline.AddLast("pre-encoder", new MessageSerializer21Bit());
            channel.Pipeline.AddLast("encoder", new MessageSerializer());
        })).LocalAddress(IPAddress.Any, availablePort);
        Log.Information("Address: {0}:{1} To: {2}:{3}", currentConfig.LocalAddress, currentConfig.LocalPort, currentConfig.ForwardAddress, currentConfig.ForwardPort);
        Log.Information("NickName: {0}", currentConfig.NickName);
        interceptor._udpBroadcaster = new UdpBroadcaster(currentConfig.LocalPort, currentConfig);
        serverBootstrap.BindAsync().ContinueWith(task => {
            if (task.IsCompletedSuccessfully) {
                interceptor._channel?.CloseAsync();
                interceptor._channel = task.GetAwaiter().GetResult();
            }
        }).ContinueWith(_ => interceptor._udpBroadcaster.StartBroadcastingAsync());
        return interceptor;
    }

    public void ShutdownAsync()
    {
        try {
            _udpBroadcaster?.Stop();
            _channel?.CloseAsync();
            AcceptorGroup.ShutdownGracefullyAsync();
            WorkerGroup.ShutdownGracefullyAsync();
        } catch {
            // ignored
        }
    }
}