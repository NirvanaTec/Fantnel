using DotNetty.Buffers;
using Nirvana.DevPlugin;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Extensions;
using Nirvana.DevPlugin.Packet;
using Serilog;

namespace Nirvana.Heypixel.Play;

public class SaClientboundSetPlayerTeamPacket : FPacket {
    
    public static readonly RegisterPacket RegisterPacket = new(EnumConnectionState.Play, EnumPacketDirection.ClientBound, 96, HeypixelProtocol.GameId, EnumProtocolVersion.V1206);
    
    private readonly string[] _invalidTeamNames = ["collideRule_1362", "collideRule_-534", "collideRule_-129", "collideRule_-180", "collideRule_-102", "collideRule_9531", "collideRule_-115", "collideRule_-403", "collideRule_1392", "collideRule_2098", "collideRule_-199", "collideRule_1176", "collideRule_-959", "collideRule_9722", "collideRule_8120", "collideRule_4063", "collideRule_-459", "collideRule_1263", "collideRule_1085", "collideRule_1742", "collideRule_-826", "collideRule_-203", "collideRule_1600", "collideRule_1963", "collideRule_3421", "collideRule_4530", "collideRule_1779", "collideRule_7943", "collideRule_-417", "collideRule_1841", "collideRule_2042", "collideRule_1232", "collideRule_-828", "collideRule_1159", "collideRule_-930", "collideRule_-132", "collideRule_-118", "collideRule_1474", "collideRule_-154", "collideRule_1856", "collideRule_-143", "collideRule_7606", "collideRule_1321", "collideRule_-167", "collideRule_7315", "collideRule_8097", "collideRule_1874", "collideRule_-137", "collideRule_8125", "collideRule_8364", "collideRule_8560", "collideRule_-198"];
    private string? _teamName;
    
    public override void ReadFromBuffer(BGameConnection connection, IByteBuffer buffer)
    {
        base.ReadFromBuffer(buffer);
        _teamName = buffer.ReadStringFromBuffer();
    }

    public override bool HandlePacket(BGameConnection connection)
    {
        if (_teamName == null) {
            return false;
        }
        var flag = _invalidTeamNames.Any(variable => variable.Equals(_teamName));
        if (flag || _teamName.StartsWith("collideRule_")) {
            Log.Information("[Heypixel] Team: {0}", _teamName);
        }
        return flag;
    }
    
}