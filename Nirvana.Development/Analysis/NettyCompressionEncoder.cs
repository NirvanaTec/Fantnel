using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using ICSharpCode.SharpZipLib.Zip.Compression;
using Nirvana.DevPlugin.Extensions;

namespace Nirvana.Development.Analysis;

public class NettyCompressionEncoder(int threshold) : MessageToByteEncoder<IByteBuffer> {
    private readonly byte[] _buffer = new byte[4096];

    private readonly Deflater _deflater = new();

    public int Threshold { get; set; } = threshold;

    protected override void Encode(IChannelHandlerContext context, IByteBuffer message, IByteBuffer output)
    {
        var readableBytes = message.ReadableBytes;
        if (readableBytes < Threshold) {
            output.WriteVarInt(0);
            output.WriteBytes(message);
            return;
        }

        _deflater.Reset();
        _deflater.SetInput(message.Array, message.ArrayOffset + message.ReaderIndex, message.ReadableBytes);
        message.SetReaderIndex(message.ReaderIndex + message.ReadableBytes);
        _deflater.Finish();
        output.WriteVarInt(readableBytes);
        while (!_deflater.IsFinished) {
            output.WriteBytes(_buffer, 0, _deflater.Deflate(_buffer));
        }
    }
}