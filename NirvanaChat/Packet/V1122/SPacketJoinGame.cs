using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V1122;

// ConnectionProtocol - 1.12.2
// PLAY.CLIENTBOUND
// SPacketJoinGame
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ClientBound, 35, EnumProtocolVersion.V1122)]
// ReSharper disable once UnusedType.Global
public class SPacketJoinGame : GameEntityIdBase;