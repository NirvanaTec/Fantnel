using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V1200;

// ConnectionProtocol - 1.20.1
// PLAY.CLIENTBOUND
// ServerboundInteractPacket
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ServerBound, 16, EnumProtocolVersion.V1200)]
// ReSharper disable once UnusedType.Global
public class ServerboundInteractPacket : AttackBase;