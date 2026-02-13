using Codexus.Development.SDK.Connection;
using Codexus.Development.SDK.Enums;
using DotNetty.Buffers;

namespace Codexus.Development.SDK.Packet;

public interface IPacket {
    EnumProtocolVersion ClientProtocolVersion { get; set; }

    void ReadFromBuffer(IByteBuffer buffer);

    void WriteToBuffer(IByteBuffer buffer);

    bool HandlePacket(GameConnection connection);
}