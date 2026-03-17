using Codexus.Development.SDK.Enums;

namespace Nirvana.Chat.Packet.V1200.Chat;

// ConnectionProtocol - 1.20.1
// PLAY.SERVERBOUND
// ServerboundChatCommandPacket
public class ServerboundChatCommandPacket() : CommandBase(EnumConnectionState.Play, EnumPacketDirection.ServerBound, 4, EnumProtocolVersion.V1200);