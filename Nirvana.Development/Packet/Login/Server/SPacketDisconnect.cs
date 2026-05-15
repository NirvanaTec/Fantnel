using DotNetty.Buffers;
using Nirvana.DevPlugin;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Extensions;
using Nirvana.DevPlugin.Packet;
using Serilog;

namespace Nirvana.Development.Packet.Login.Server;

public class SPacketDisconnect : FPacket {
    public static readonly RegisterPacket RegisterPacket = new(EnumConnectionState.Login, EnumPacketDirection.ClientBound, 0);

    private string? _reason;

    public override void ReadFromBuffer(BGameConnection connection, IByteBuffer buffer)
    {
        base.ReadFromBuffer(buffer);
        _reason = buffer.ReadStringFromBuffer();
    }

    public override bool HandlePacket(BGameConnection connection)
    {
        Log.Debug("Disconnect Reason: {0}", _reason);
        return false;
    }
}