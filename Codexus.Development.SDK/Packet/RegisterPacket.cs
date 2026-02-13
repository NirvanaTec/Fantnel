using Codexus.Development.SDK.Enums;

namespace Codexus.Development.SDK.Packet;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterPacket(
    EnumConnectionState state,
    EnumPacketDirection direction,
    int[] packetIds,
    EnumProtocolVersion[] versions,
    bool skip = false) : Attribute {
    public RegisterPacket(EnumConnectionState state, EnumPacketDirection direction, int packetId, bool skip = false)
        : this(state, direction, [packetId], Enum.GetValues<EnumProtocolVersion>(), skip)
    {
    }

    public RegisterPacket(EnumConnectionState state, EnumPacketDirection direction, int[] packetIds, bool skip = false)
        : this(state, direction, packetIds, Enum.GetValues<EnumProtocolVersion>(), skip)
    {
    }

    public RegisterPacket(EnumConnectionState state, EnumPacketDirection direction, int packetId,
        EnumProtocolVersion version, bool skip = false)
        : this(state, direction, [packetId], [version], skip)
    {
    }

    public RegisterPacket(EnumConnectionState state, EnumPacketDirection direction, int[] packetIds,
        EnumProtocolVersion version, bool skip = false)
        : this(state, direction, packetIds, [version], skip)
    {
    }

    public RegisterPacket(EnumConnectionState state, EnumPacketDirection direction, int packetId,
        EnumProtocolVersion[] versions, bool skip = false)
        : this(state, direction, [packetId], versions, skip)
    {
    }

    public EnumConnectionState State { get; } = state;

    public EnumPacketDirection Direction { get; } = direction;

    public int[] PacketIds { get; } = packetIds;

    public EnumProtocolVersion[] Versions { get; } = versions;

    public bool Skip { get; } = skip;
}