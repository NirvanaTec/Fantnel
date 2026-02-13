using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V108X;

// ConnectionProtocol - 1.8.9
// PLAY.CLIENTBOUND
// S01PacketJoinGame
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ClientBound, 1, EnumProtocolVersion.V108X)]
// ReSharper disable once UnusedType.Global
public class S01PacketJoinGame : GameEntityIdBase;