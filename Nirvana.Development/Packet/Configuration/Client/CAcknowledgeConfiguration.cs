using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Packet;

namespace Nirvana.Development.Packet.Configuration.Client;

public class CAcknowledgeConfiguration : DPacket {
    public static readonly RegisterPacket RegisterPacket = new(EnumConnectionState.Play, EnumPacketDirection.ServerBound, 12, EnumProtocolVersion.V1206, EnumProtocolVersion.V1210);
}