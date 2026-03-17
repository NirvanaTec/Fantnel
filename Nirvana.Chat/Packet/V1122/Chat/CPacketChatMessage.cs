using Codexus.Development.SDK.Enums;

namespace Nirvana.Chat.Packet.V1122.Chat;

// ConnectionProtocol - 1.12.2
// PLAY.SERVERBOUND
// CPacketChatMessage
public class CPacketChatMessage() : CommandBase(EnumConnectionState.Play, EnumPacketDirection.ServerBound, 2, EnumProtocolVersion.V1122);