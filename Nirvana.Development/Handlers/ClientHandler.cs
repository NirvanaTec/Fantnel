using Codexus.Development.SDK.Connection;
using Codexus.Development.SDK.Enums;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace Nirvana.Development.Handlers;

public class ClientHandler(GameConnection connection) : ChannelHandlerAdapter {
    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        if (message is not IByteBuffer buffer) {
            return;
        }

        var processedBuffer = OnServerReceived(buffer);
        if (processedBuffer == null) {
            return;
        }

        base.ChannelRead(context, processedBuffer);
    }

    private IByteBuffer? OnServerReceived(IByteBuffer buffer)
    {
        return ServerHandler.HandlePacketReceived(connection, buffer, EnumPacketDirection.ClientBound);
    }
}