using Codexus.Development.SDK.Connection;
using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;
using DotNetty.Buffers;
using NirvanaChat.Message;
using Serilog;

namespace NirvanaChat.Packet;

public class GameEntityIdBase : IPacket {
    private byte[]? _rawBytes;
    private static int EntityId { get; set; }
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
        ChatMessage.Join(connection, EntityId);
        ChatMessage.OnHeartbeat(null);
        Log.Information("[IRC] EntityId: {EntityId}", EntityId);
        return false;
    }
}