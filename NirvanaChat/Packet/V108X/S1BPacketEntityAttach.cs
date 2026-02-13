using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V108X;

// ConnectionProtocol - 1.8.9
// PLAY.CLIENTBOUND
// S1BPacketEntityAttach
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ClientBound, 27, EnumProtocolVersion.V108X)]
// ReSharper disable once UnusedType.Global
public class S1BPacketEntityAttach : AttackBase;