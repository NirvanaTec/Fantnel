using System.Text;
using System.Text.Json;
using Codexus.Development.SDK.Connection;
using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Extensions;
using DotNetty.Buffers;
using NirvanaChat.Utils;
using Serilog;

namespace NirvanaChat;

public class PacketTools {
    public static void SendGameMessage(string message, GameConnection argsConnection)
    {
        try {
            var buffer = Unpooled.Buffer();

            if (argsConnection.ProtocolVersion >= EnumProtocolVersion.V1200) {
                // 1.20.1
                // ClientboundSystemChatPacket
                // 99 + 9 = 108
                buffer.WriteVarInt(108);
                buffer.WriteByte(0x08);
                var textBytes = Encoding.UTF8.GetBytes(message);
                buffer.WriteShort(textBytes.Length);
                buffer.WriteBytes(textBytes);
                buffer.WriteBoolean(false);
            } else {
                var entity = MinecraftColorCodeConverter.ParseColoredString(message);
                var jsonMessage = JsonSerializer.Serialize(entity);
                var textBytes = Encoding.UTF8.GetBytes(jsonMessage);
                if (argsConnection.ProtocolVersion == EnumProtocolVersion.V1180) {
                    // 1.18.1
                    // ClientboundChatPacket
                    buffer.WriteVarInt(15);
                    buffer.WriteByte(0x08);
                    buffer.WriteShort(textBytes.Length);
                    buffer.WriteBytes(textBytes);
                    buffer.WriteVarInt(1);
                    var uuidBytes = Guid.NewGuid().ToByteArray();
                    buffer.WriteBytes(uuidBytes);
                } else if (argsConnection.ProtocolVersion == EnumProtocolVersion.V1122) {
                    // 1.12.2
                    // SPacketChat
                    buffer.WriteVarInt(15);
                    buffer.WriteStringToBuffer(jsonMessage);
                    buffer.WriteVarInt(1);
                } else if (argsConnection.ProtocolVersion == EnumProtocolVersion.V108X) {
                    // 1.8.9
                    // S02PacketChat
                    buffer.WriteVarInt(2);
                    buffer.WriteStringToBuffer(jsonMessage);
                    buffer.WriteVarInt(1);
                }
            }

            argsConnection.ClientChannel.WriteAndFlushAsync(buffer);
        } catch (Exception e) {
            Log.Error("[IRC] 发送消息失败\n{message}", e.Message);
        }
    }
}