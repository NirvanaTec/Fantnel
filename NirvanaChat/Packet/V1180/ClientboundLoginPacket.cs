using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V1180;

// ConnectionProtocol - 1.18.1
// PLAY.CLIENTBOUND
// ClientboundLoginPacket
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ClientBound, 38, EnumProtocolVersion.V1180)]
// ReSharper disable once UnusedType.Global
public class ClientboundLoginPacket : GameEntityIdBase;