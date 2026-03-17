using Codexus.Development.SDK.Enums;

namespace Nirvana.Chat.Packet.V1180.Chat;

// ConnectionProtocol - 1.18.1
// PLAY.SERVERBOUND
// ServerboundChatPacket
public class ServerboundChatPacket() : CommandBase(EnumConnectionState.Play, EnumPacketDirection.ServerBound, 3, EnumProtocolVersion.V1180);