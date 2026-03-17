using Codexus.Development.SDK.Enums;

namespace Nirvana.Chat.Packet.V1206.Chat;

// ConnectionProtocol > GameProtocols - 1.20.6
// PLAY.SERVERBOUND
// ServerboundChatCommandPacket
public class ServerboundChatCommandPacket() : CommandBase(EnumConnectionState.Play, EnumPacketDirection.ServerBound, 4, EnumProtocolVersion.V1206);