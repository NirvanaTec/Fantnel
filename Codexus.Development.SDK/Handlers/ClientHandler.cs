using Codexus.Development.SDK.Connection;
using Codexus.Development.SDK.Utils;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace Codexus.Development.SDK.Handlers;

public class ClientHandler(GameConnection connection) : ChannelHandlerAdapter {
    public override void ChannelActive(IChannelHandlerContext context)
    {
        context.Channel.GetAttribute(ChannelAttribute.Connection).Set(connection);
    }

    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        connection.OnServerReceived((IByteBuffer)message);
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
        context.Channel.GetAttribute(ChannelAttribute.Connection).Remove();
    }
}