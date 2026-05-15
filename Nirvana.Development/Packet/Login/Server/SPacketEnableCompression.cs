using System;
using DotNetty.Buffers;
using Nirvana.Development.Connection;
using Nirvana.DevPlugin;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Extensions;
using Nirvana.DevPlugin.Packet;

namespace Nirvana.Development.Packet.Login.Server;

public class SPacketEnableCompression : DPacket {
    public static readonly RegisterPacket RegisterPacket = new(EnumConnectionState.Login, EnumPacketDirection.ClientBound, 3);

    private int CompressionThreshold { get; set; }

    public override void ReadFromBuffer(BGameConnection connection, IByteBuffer buffer)
    {
        CompressionThreshold = buffer.ReadVarIntFromBuffer();
    }

    public override bool HandlePacket(BGameConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection.ServerChannel);
        GameConnection.EnableCompression(connection.ServerChannel, CompressionThreshold);
        return true;
    }
}