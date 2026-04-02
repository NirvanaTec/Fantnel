using Nirvana.Chat.Manager;
using Nirvana.Chat.Packet.V108X.Chat;
using Nirvana.Chat.Packet.V1122.Chat;
using Nirvana.Chat.Packet.V1180.Chat;
using Nirvana.Chat.Packet.V1200.Chat;
using Nirvana.Development.Manager;
using Nirvana.Development.Packet.IPacket;

namespace Nirvana.Chat;

public static class ChatPluginMain {
    private static readonly List<APacket> Packets = [
        new C01PacketChatMessage(),
        // 1122
        new CPacketChatMessage(),
        // 1180
        new ServerboundChatPacket(),
        // 1200
        new ServerboundChatCommandPacket(),
        // 1206
        new Packet.V1206.Chat.ServerboundChatCommandPacket()
    ];

    public static void Initialize()
    {
        NPacketManager.RegisterPacketFromList("Nirvana.Chat", Packets, ChatManager.Register);
    }
}