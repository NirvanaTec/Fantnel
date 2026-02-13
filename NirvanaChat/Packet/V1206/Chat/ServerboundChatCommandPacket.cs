using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V1206.Chat;

// ConnectionProtocol > GameProtocols - 1.20.6
// PLAY.SERVERBOUND
// ServerboundChatCommandPacket
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ServerBound, 4, EnumProtocolVersion.V1206)]
// ReSharper disable once UnusedType.Global
public class ServerboundChatCommandPacket : CommandBase;