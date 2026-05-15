using Nirvana.DevPlugin;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Packet;
using Serilog;

namespace Nirvana.Development.Packet.Login.Client;

public class CPacketLoginAcknowledged : DPacket {
    public static readonly RegisterPacket RegisterPacket = new(EnumConnectionState.Login, EnumPacketDirection.ServerBound, 3, EnumProtocolVersion.V1210, EnumProtocolVersion.V1206);

    public override bool HandlePacket(BGameConnection connection)
    {
        // if (EventManager.TriggerEvent<IEventLoginSuccess>(success => success.OnLoginSuccess(), ProtocolVersion) != null) {
        //     return true;
        // }

        connection.State = EnumConnectionState.Configuration;
        Log.Information("Login Acknowledged.");
        return false;
    }
}