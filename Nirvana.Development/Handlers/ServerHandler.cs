using Codexus.Development.SDK.Connection;
using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Extensions;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Nirvana.Development.Manager;
using Serilog;

namespace Nirvana.Development.Handlers;

public class ServerHandler(GameConnection connection) : ChannelHandlerAdapter {
    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        if (message is not IByteBuffer buffer) {
            return;
        }

        var processedBuffer = OnClientReceived(buffer);
        if (processedBuffer == null) {
            return;
        }

        base.ChannelRead(context, processedBuffer);
    }

    private IByteBuffer? OnClientReceived(IByteBuffer buffer)
    {
        return HandlePacketReceived(connection, buffer, EnumPacketDirection.ServerBound);
    }

    public static IByteBuffer? HandlePacketReceived(
        GameConnection gameConnection,
        IByteBuffer buffer,
        EnumPacketDirection direction)
    {
        buffer.MarkReaderIndex();

        var packetId = buffer.ReadVarIntFromBuffer();
        var packets = NPacketManager.Instance.BuildPacket(gameConnection.State, direction, gameConnection.ProtocolVersion, packetId);

        foreach (var packet in packets) {
            if (packet.Skip) {
                continue;
            }

            packet.ClientProtocolVersion = gameConnection.ProtocolVersion;
            try {
                packet.ReadFromBuffer(buffer);
            } catch (Exception ex) {
                var objArray = new object[] {
                    direction,
                    packetId,
                    packet,
                    gameConnection.ProtocolVersion
                };
                Log.Error(ex, "Cannot read packet from buffer, direction: {Direction}, Id: {Id}, Packet: {Packet}, ProtocolVersion: {ProtocolVersion}", objArray);
                throw;
            }

            if (packet.HandlePacket(gameConnection)) {
                buffer.ResetReaderIndex();
                return null;
            }

            buffer.Clear();
            buffer.WriteVarInt(packetId);
            buffer.ReadVarIntFromBuffer();
            packet.WriteToBuffer(buffer);
        }

        buffer.ResetReaderIndex();
        return buffer;
    }
}