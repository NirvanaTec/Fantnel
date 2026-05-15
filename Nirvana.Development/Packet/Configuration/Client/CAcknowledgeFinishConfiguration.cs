using Nirvana.DevPlugin;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Packet;
using Serilog;

namespace Nirvana.Development.Packet.Configuration.Client;

public class CAcknowledgeFinishConfiguration : DPacket {
    public static readonly RegisterPacket RegisterPacket = new(EnumConnectionState.Configuration, EnumPacketDirection.ServerBound, 3, EnumProtocolVersion.V1206, EnumProtocolVersion.V1210);

    public override bool HandlePacket(BGameConnection connection)
    {
        connection.State = EnumConnectionState.Play;
        Log.Information("Finished Configuration.");
        return false;
    }
}