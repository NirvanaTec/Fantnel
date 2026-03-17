using Codexus.Development.SDK.Enums;

namespace Nirvana.Chat.Packet.V108X.Chat;

// ConnectionProtocol - 1.8.9
// PLAY.SERVERBOUND
// C01PacketChatMessage
public class C01PacketChatMessage() : CommandBase(EnumConnectionState.Play, EnumPacketDirection.ServerBound, 1, EnumProtocolVersion.V108X);