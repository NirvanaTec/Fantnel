using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V1122;

// ConnectionProtocol - 1.12.2
// PLAY.CLIENTBOUND
// SPacketEntityAttach
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ClientBound, 61, EnumProtocolVersion.V1122)]
// ReSharper disable once UnusedType.Global
public class SPacketEntityAttach : AttackBase;