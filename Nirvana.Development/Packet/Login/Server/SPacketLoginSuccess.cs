using System;
using DotNetty.Buffers;
using Nirvana.Development.Manager;
using Nirvana.DevPlugin;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Events.Event;
using Nirvana.DevPlugin.Extensions;
using Nirvana.DevPlugin.Packet;
using Serilog;

namespace Nirvana.Development.Packet.Login.Server;

public class SPacketLoginSuccess : FPacket {
    public static readonly RegisterPacket RegisterPacket = new(EnumConnectionState.Login, EnumPacketDirection.ClientBound, 2);

    private readonly byte[] _guid = new byte[16];
    private string? _username;
    private string? _uuid;

    public override void ReadFromBuffer(BGameConnection connection, IByteBuffer buffer)
    {
        base.ReadFromBuffer(buffer);
        if (ProtocolVersion > EnumProtocolVersion.V1180) {
            buffer.ReadBytes(_guid);
        } else {
            _uuid = buffer.ReadStringFromBuffer(36);
        }

        _username = buffer.ReadStringFromBuffer(16);
    }

    public override bool HandlePacket(BGameConnection connection)
    {
        if (ProtocolVersion > EnumProtocolVersion.V1180) {
            Log.Information("Joined: {0}[{1}]", _username, new Guid(_guid, true));
        } else {
            Log.Information("Joined: {0}[{1}]", _username, _uuid);
        }

        if (ProtocolVersion > EnumProtocolVersion.V1200) {
            return false;
        }

        connection.State = EnumConnectionState.Play;
        return EventManager.TriggerEvent<IEventLoginSuccess>(success => success.OnLoginSuccess(connection), ProtocolVersion) != null;
    }
}