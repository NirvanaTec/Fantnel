using DotNetty.Buffers;

namespace Nirvana.DevPlugin.Packet;

public abstract class DPacket : BPacket {
    public override void ReadFromBuffer(BGameConnection connection, IByteBuffer buffer) { }

    public override void WriteToBuffer(IByteBuffer buffer) { }
}