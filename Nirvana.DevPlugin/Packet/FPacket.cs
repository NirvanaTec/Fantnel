using DotNetty.Buffers;

namespace Nirvana.DevPlugin.Packet;

public abstract class FPacket : BPacket {
    private byte[]? _rawBytes;

    public override void ReadFromBuffer(BGameConnection connection, IByteBuffer buffer)
    {
        ReadFromBuffer(buffer);
    }

    protected void ReadFromBuffer(IByteBuffer buffer)
    {
        _rawBytes = new byte[buffer.ReadableBytes];
        buffer.GetBytes(buffer.ReaderIndex, _rawBytes);
    }

    public override void WriteToBuffer(IByteBuffer buffer)
    {
        if (_rawBytes != null) {
            buffer.WriteBytes(_rawBytes);
        }
    }
}