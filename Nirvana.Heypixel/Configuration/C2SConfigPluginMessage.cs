using DotNetty.Buffers;
using Nirvana.DevPlugin;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Extensions;
using Nirvana.DevPlugin.Packet;
using Serilog;

namespace Nirvana.Heypixel.Configuration;

public class C2SConfigPluginMessage : BPacket {
    public static readonly RegisterPacket RegisterPacket = new(EnumConnectionState.Configuration, EnumPacketDirection.ServerBound, 2, HeypixelProtocol.GameId, EnumProtocolVersion.V1206);

    private string? _identifier;
    private byte[]? _payload;
    
    public override void ReadFromBuffer(BGameConnection connection, IByteBuffer buffer)
    {
        _identifier = buffer.ReadStringFromBuffer(32);
        _payload = buffer.ReadBytes();
    }
    
    public override void WriteToBuffer(IByteBuffer buffer)
    {
        ArgumentNullException.ThrowIfNull(_identifier);
        buffer.WriteStringToBuffer(_identifier);
        buffer.WriteBytes(_payload);
    }

    public override bool HandlePacket(BGameConnection connection)
    {
        // if (_identifier == "minecraft:register") {
        //     Log.Information("[Heypixel] Register 2");
        //     return true;
        // } 
        if (_identifier == "minecraft:brand") {
            Log.Information("[Heypixel] Minecraft");
            ArgumentNullException.ThrowIfNull(_payload);
            _payload = Convert.FromBase64String("BWZvcmdl");
        }

        return false;
    }
}