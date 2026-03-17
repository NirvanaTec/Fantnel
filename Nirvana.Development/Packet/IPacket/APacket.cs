using Codexus.Development.SDK.Connection;
using Codexus.Development.SDK.Enums;
using DotNetty.Buffers;

namespace Nirvana.Development.Packet.IPacket;

public abstract class APacket {
    public readonly EnumPacketDirection Direction;
    public readonly int[] PacketIds;

    public readonly bool Skip;
    public readonly EnumConnectionState State;
    public readonly EnumProtocolVersion[] Versions;

    // ReSharper disable once UnusedMember.Global
    protected APacket(EnumConnectionState state, EnumPacketDirection direction, int[] packetIds, EnumProtocolVersion[] versions, bool skip = false)
    {
        State = state;
        Direction = direction;
        PacketIds = packetIds;
        Versions = versions;
        Skip = skip;
    }

    protected APacket(EnumConnectionState state, EnumPacketDirection direction, int packetId, EnumProtocolVersion version, bool skip = false)
    {
        State = state;
        Direction = direction;
        PacketIds = [packetId];
        Versions = [version];
        Skip = skip;
    }

    public EnumProtocolVersion ClientProtocolVersion { get; set; }
    public abstract void ReadFromBuffer(IByteBuffer buffer);
    public abstract void WriteToBuffer(IByteBuffer buffer);
    public abstract bool HandlePacket(GameConnection connection);
}