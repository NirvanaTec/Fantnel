using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V1180;

// ConnectionProtocol - 1.18.1
// PLAY.SERVERBOUND
// ServerboundInteractPacket
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ServerBound, 13, EnumProtocolVersion.V1180)]
// ReSharper disable once UnusedType.Global
public class ServerboundInteractPacket : AttackBase;