using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V1206.Chat;

// ConnectionProtocol > GameProtocols - 1.20.6
// PLAY.CLIENTBOUND
// ClientboundPlayerInfoUpdatePacket
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ClientBound, 62, EnumProtocolVersion.V1206)]
// ReSharper disable once UnusedType.Global
public class ClientboundPlayerInfoUpdatePacket : TabBase;