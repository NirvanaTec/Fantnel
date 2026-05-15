using Nirvana.DevPlugin;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Packet;
using Serilog;

namespace Nirvana.Development.Packet.Configuration.Server;

public class SStartConfiguration : DPacket {
    public static readonly RegisterPacket RegisterPacket = new(EnumConnectionState.Play, EnumPacketDirection.ClientBound, 105, EnumProtocolVersion.V1206, EnumProtocolVersion.V1210);

    public override bool HandlePacket(BGameConnection connection)
    {
        connection.State = EnumConnectionState.Configuration;
        Log.Information("Starting Configuration.");
        return false;
    }
}