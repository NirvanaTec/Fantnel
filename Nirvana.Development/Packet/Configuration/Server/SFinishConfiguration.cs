using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Packet;

namespace Nirvana.Development.Packet.Configuration.Server;

public class SFinishConfiguration : DPacket {
    public static readonly RegisterPacket RegisterPacket = new(EnumConnectionState.Configuration, EnumPacketDirection.ClientBound, 3, EnumProtocolVersion.V1206, EnumProtocolVersion.V1210);
}