using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Nirvana.Development.Connection;
using Nirvana.Development.Utils;

namespace Nirvana.Development.Handlers;

public class ClientHandler(GameConnection connection) : ChannelHandlerAdapter {
    public override void ChannelActive(IChannelHandlerContext context)
    {
        context.Channel.GetAttribute(ChannelAttribute.Connection).Set(connection);
    }

    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        if (message is IByteBuffer buffer) {
            connection.OnServerReceived(buffer);
        }
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
        context.Channel.GetAttribute(ChannelAttribute.Connection).Remove();
    }
}