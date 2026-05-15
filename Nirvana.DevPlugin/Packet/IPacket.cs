using DotNetty.Buffers;
using Nirvana.DevPlugin.Enums;

namespace Nirvana.DevPlugin.Packet;

public interface IPacket {
    int PacketId { get; set; }

    EnumProtocolVersion ProtocolVersion { get; set; }

    void ReadFromBuffer(BGameConnection connection, IByteBuffer buffer);

    void WriteToBuffer(IByteBuffer buffer);

    /**
     * true: 不向白端发送包
     * false: 向白端发送包
     */
    bool HandlePacket(BGameConnection connection)
    {
        return false;
    }
}