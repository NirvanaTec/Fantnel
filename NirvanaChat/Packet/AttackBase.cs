using Codexus.Development.SDK.Connection;
using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;
using DotNetty.Buffers;
using NirvanaChat.Message;
using Serilog;

namespace NirvanaChat.Packet;

public class AttackBase : IPacket {
    private byte[]? _rawBytes;
// 做事要讲良心 | 做事要讲良心 | 做事要讲良心

    private int EntityId { get; set; }
    public EnumProtocolVersion ClientProtocolVersion { get; set; }

    public void ReadFromBuffer(IByteBuffer buffer)
    {
        _rawBytes = new byte[buffer.ReadableBytes];
        buffer.GetBytes(buffer.ReaderIndex, _rawBytes);

        EntityId = buffer.ReadInt();
    }

    public void WriteToBuffer(IByteBuffer buffer)
    {
        if (_rawBytes != null) {
            buffer.WriteBytes(_rawBytes);
        }
    }

    public bool HandlePacket(GameConnection connection)
    {
        Log.Information("[IRC] Attack: {EntityId}", EntityId);
        var players = ChatMessage.GetPlayer(connection.GameId, EntityId);
        if (players == null) {
            return false;
        }

        if (players.Value.Item1.FriendlyMode) {
            PacketTools.SendGameMessage("他开启了友好模式，不能攻击", connection);
            return true;
        }

        return false;
    }
}