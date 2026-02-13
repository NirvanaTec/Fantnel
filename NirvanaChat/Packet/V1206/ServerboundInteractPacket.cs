using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V1206;

// ConnectionProtocol > GameProtocols - 1.20.6
// PLAY.CLIENTBOUND
// ServerboundInteractPacket
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ServerBound, 22, EnumProtocolVersion.V1206)]
// ReSharper disable once UnusedType.Global
public class ServerboundInteractPacket : AttackBase;