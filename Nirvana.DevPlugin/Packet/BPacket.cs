using DotNetty.Buffers;
using Nirvana.DevPlugin.Enums;

namespace Nirvana.DevPlugin.Packet;

public abstract class BPacket : IPacket {
    public int PacketId { get; set; }
    public EnumProtocolVersion ProtocolVersion { get; set; }
    public abstract void ReadFromBuffer(BGameConnection connection, IByteBuffer buffer);
    public abstract void WriteToBuffer(IByteBuffer buffer);

    public virtual bool HandlePacket(BGameConnection connection)
    {
        return false;
    }
}