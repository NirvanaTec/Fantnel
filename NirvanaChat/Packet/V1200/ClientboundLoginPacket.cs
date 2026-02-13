using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V1200;

// ConnectionProtocol - 1.20.1
// PLAY.CLIENTBOUND
// ClientboundLoginPacket
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ClientBound, 40, EnumProtocolVersion.V1200)]
// ReSharper disable once UnusedType.Global
public class ClientboundLoginPacket : GameEntityIdBase;