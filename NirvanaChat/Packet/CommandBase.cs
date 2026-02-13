using Codexus.Development.SDK.Connection;
using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Extensions;
using Codexus.Development.SDK.Packet;
using DotNetty.Buffers;
using NirvanaAPI;
using NirvanaChat.Message;

namespace NirvanaChat.Packet;

public class CommandBase : IPacket {
    private string _command = string.Empty;
    private bool _isCommand;

    private byte[]? _rawBytes;

    public EnumProtocolVersion ClientProtocolVersion { get; set; }

    public void ReadFromBuffer(IByteBuffer buffer)
    {
        _rawBytes = new byte[buffer.ReadableBytes];
        buffer.GetBytes(buffer.ReaderIndex, _rawBytes);

        _command = buffer.ReadStringFromBuffer();
        _isCommand = IsIrc(_command);
    }

    public void WriteToBuffer(IByteBuffer buffer)
    {
        if (_isCommand && NirvanaConfig.Config.ChatEnable) {
            return;
        }

        if (_rawBytes != null) {
            buffer.WriteBytes(_rawBytes);
        }
    }

    public bool HandlePacket(GameConnection connection)
    {
        if (!_isCommand || !NirvanaConfig.Config.ChatEnable) {
            return false;
        }

        // "/irc 123" > "123"
        // "irc 123" > "123 " > "123"
        var content = _command.Length > 4 ? _command[4..].Trim() : string.Empty;

        if (string.IsNullOrWhiteSpace(content)) {
            PacketTools.SendGameMessage("§e[IRC]: 请使用 /irc <消息>", connection);
            return true;
        }

        if (ChatMessage.Account.IsNullOrEmpty()) {
            PacketTools.SendGameMessage("§e[IRC]: 请先登录账号", connection);
            return true;
        }

        ChatMessage.SendMessage(content);
        return true;
    }

    private static bool IsIrc(string command)
    {
        // 1.20.1
        if (command.StartsWith("irc ", StringComparison.OrdinalIgnoreCase)) {
            return true;
        }

        if (command.Equals("irc", StringComparison.OrdinalIgnoreCase)) {
            return true;
        }

        // 1.18.1 | 1.12.2
        return command.StartsWith("/irc ", StringComparison.OrdinalIgnoreCase) ||
               command.Equals("/irc", StringComparison.OrdinalIgnoreCase);
    }
}