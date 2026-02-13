using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V1180.Chat;

// ConnectionProtocol - 1.18.1
// PLAY.SERVERBOUND
// ServerboundChatPacket
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ServerBound, 3, EnumProtocolVersion.V1180)]
// ReSharper disable once UnusedType.Global
public class ServerboundChatPacket : CommandBase;