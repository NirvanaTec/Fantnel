using System;
using DotNetty.Buffers;
using Nirvana.Development.Manager;
using Nirvana.DevPlugin;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Events.Event;
using Nirvana.DevPlugin.Extensions;
using Nirvana.DevPlugin.Packet;

namespace Nirvana.Development.Packet.Base1200.Play.Server.Configuration;

public class SPacketPluginMessage : BPacket {
    public static readonly RegisterPacket RegisterPacket = new(EnumConnectionState.Configuration, EnumPacketDirection.ClientBound, 1, EnumProtocolVersion.V1206);

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
        ArgumentNullException.ThrowIfNull(_identifier);
        ArgumentNullException.ThrowIfNull(_payload);
        return EventManager.TriggerEvent<IEventPluginMessage>(message => {
            message.Identifier = _identifier;
            message.Payload = _payload;
            var result = message.OnPluginMessage(connection);
            _identifier = message.Identifier;
            _payload = message.Payload;
            return result;
        }, ProtocolVersion) != null;
    }
}