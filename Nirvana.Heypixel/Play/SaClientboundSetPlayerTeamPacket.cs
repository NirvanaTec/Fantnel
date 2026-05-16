using DotNetty.Buffers;
using Nirvana.DevPlugin;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Extensions;
using Nirvana.DevPlugin.Packet;
using Serilog;

namespace Nirvana.Heypixel.Play;

public class SaClientboundSetPlayerTeamPacket : FPacket {
    
    public static readonly RegisterPacket RegisterPacket = new(EnumConnectionState.Play, EnumPacketDirection.ClientBound, 96, HeypixelProtocol.GameId, EnumProtocolVersion.V1206);

    private string? _invalidTeamName; // 无效团队名称
    private string? _teamName; // 当前团队名称
    
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
        
        if (_invalidTeamName == null) {
            if (_teamName.StartsWith("collideRule_")) {
                _invalidTeamName = _teamName;
            }
        }

        if (_teamName.Equals(_invalidTeamName)) {
            Log.Information("[Heypixel] Team: {0}", _teamName);
            return true;
        }
        
        return false;
    }
    
}