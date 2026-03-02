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

        var player1 = players.Value;
        var flag = TabBase.List.Any(tabBase =>
            !string.IsNullOrEmpty(tabBase.OldName) && tabBase.OldName.Equals(player1.Item2.NickName));

        if (flag && player1.Item1.FriendlyMode) {
            PacketTools.SendGameMessage("他开启了友好模式，不能攻击", connection);
            Log.Information("他开启了友好模式，不能攻击 {EntityId}", EntityId);
            return true;
        }

        return false;
    }
}