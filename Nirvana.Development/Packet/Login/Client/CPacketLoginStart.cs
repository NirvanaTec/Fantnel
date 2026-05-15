using System;
using DotNetty.Buffers;
using Nirvana.DevPlugin;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Extensions;
using Nirvana.DevPlugin.Packet;
using Serilog;

namespace Nirvana.Development.Packet.Login.Client;

public class CPacketLoginStart : BPacket {
    public static readonly RegisterPacket RegisterPacket = new(EnumConnectionState.Login, EnumPacketDirection.ServerBound, 0);
    private byte[]? _rawBytes;

    private string? _userName;

    public override void ReadFromBuffer(BGameConnection connection, IByteBuffer buffer)
    {
        _userName = buffer.ReadStringFromBuffer(16);
        _rawBytes = buffer.ReadBytes();
    }

    public override void WriteToBuffer(IByteBuffer buffer)
    {
        ArgumentNullException.ThrowIfNull(_userName);
        buffer.WriteStringToBuffer(_userName);
        buffer.WriteBytes(_rawBytes);
    }

    public override bool HandlePacket(BGameConnection connection)
    {
        Log.Information("Trying Login: {0} > {1}", _userName, connection.Config.NickName);
        _userName = connection.Config.NickName;
        return false;
    }
}