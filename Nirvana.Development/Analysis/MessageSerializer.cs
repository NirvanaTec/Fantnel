using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Nirvana.DevPlugin.Extensions;
using Nirvana.DevPlugin.Packet;

namespace Nirvana.Development.Analysis;

public class MessageSerializer : MessageToByteEncoder<IPacket> {
    protected override void Encode(IChannelHandlerContext context, IPacket message, IByteBuffer output)
    {
        output.WriteVarInt(message.PacketId);
        message.WriteToBuffer(output);
    }
}