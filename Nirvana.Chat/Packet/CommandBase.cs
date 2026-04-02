using Codexus.Development.SDK.Connection;
using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Extensions;
using DotNetty.Buffers;
using Nirvana.Chat.Message;
using Nirvana.Development.Packet.IPacket;
using NirvanaAPI;

namespace Nirvana.Chat.Packet;

public class CommandBase : APacket {
    private string _command = string.Empty;
    private bool _isCommand;

    private byte[]? _rawBytes;

    protected CommandBase(EnumConnectionState state, EnumPacketDirection direction, int packetId, EnumProtocolVersion version, bool skip = false) : base(state, direction, packetId, version, skip)
    {
    }

    public override void ReadFromBuffer(IByteBuffer buffer)
    {
        _rawBytes = new byte[buffer.ReadableBytes];
        buffer.GetBytes(buffer.ReaderIndex, _rawBytes);

        _command = buffer.ReadStringFromBuffer(short.MaxValue);
        _isCommand = IsIrc(_command);
    }

    public override void WriteToBuffer(IByteBuffer buffer)
    {
        if (_isCommand && NirvanaConfig.GetBool("chatEnable")) {
            return;
        }

        if (_rawBytes != null) {
            buffer.WriteBytes(_rawBytes);
        }
    }

    public override bool HandlePacket(GameConnection connection)
    {
        if (!_isCommand || !NirvanaConfig.GetBool("chatEnable")) {
            return false;
        }

        // "/irc 123" > "123"
        // "irc 123" > "123 " > "123"
        var content = _command.Length > 4 ? _command[4..].Trim() : string.Empty;

        if (string.IsNullOrWhiteSpace(content)) {
            PacketTools.SendGameMessage("§e[IRC]: 请使用 /irc <消息>", connection);
            return true;
        }

        try {
            NirvanaConfig.IsLogin(); // 检查是否登录
        } catch (Exception) {
            PacketTools.SendGameMessage("§e[IRC]: 请先登录账号", connection);
            return true;
        }

        ChatMessage.SendMessage(content);
        return true;
    }

    private static bool IsIrc(string command)
    {
        var commandsPrefix = new List<string> {
            "irc", "chat"
        };

        foreach (var prefix in commandsPrefix) {
            // 1.20.1
            if (command.StartsWith($"{prefix} ", StringComparison.OrdinalIgnoreCase)) {
                return true;
            }

            // 1.20.1
            if (command.Equals($"{prefix}", StringComparison.OrdinalIgnoreCase)) {
                return true;
            }

            // 1.18.1 | 1.12.2
            if (command.StartsWith($"/{prefix} ", StringComparison.OrdinalIgnoreCase)) {
                return true;
            }

            // 1.18.1 | 1.12.2
            if (command.Equals($"/{prefix}", StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }

        return false;
    }
}