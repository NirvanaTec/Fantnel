using Codexus.Development.SDK.Enums;
using Nirvana.Development.Packet.IPacket;

namespace Nirvana.Development.Manager;

public class NPacketManager {
    public static readonly NPacketManager Instance = new();
    private readonly Dictionary<string, APacket[]> _packets = new();

    public static void RegisterPacketFromList(string pluginId, APacket[] packets)
    {
        Instance._packets[pluginId] = packets;
    }

    public List<APacket> BuildPacket(EnumConnectionState state, EnumPacketDirection direction, EnumProtocolVersion version, int packetId)
    {
        return (from item in _packets.Values from packet in item where packet.State == state && packet.Direction == direction from id in packet.PacketIds where id == packetId from ver in packet.Versions where ver == version select packet).ToList();
    }
}