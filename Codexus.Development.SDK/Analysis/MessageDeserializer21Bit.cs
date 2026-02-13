using Codexus.Development.SDK.Extensions;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Serilog;

namespace Codexus.Development.SDK.Analysis;

public class MessageDeserializer21Bit : ByteToMessageDecoder {
    protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
    {
        message.MarkReaderIndex();
        var array = new byte[3];
        for (var i = 0; i < 3; i++) {
            if (!message.IsReadable()) {
                message.ResetReaderIndex();
                break;
            }

            array[i] = message.ReadByte();
            if (array[i] >= 128) {
                continue;
            }

            try {
                var num = array.ReadVarInt();
                if (message.ReadableBytes >= num) {
                    output.Add(message.ReadBytes(num));
                } else {
                    message.ResetReaderIndex();
                }

                break;
            } catch (Exception exception) {
                Log.Error(exception, "Failed to decode message.");
                break;
            }
        }
    }
}