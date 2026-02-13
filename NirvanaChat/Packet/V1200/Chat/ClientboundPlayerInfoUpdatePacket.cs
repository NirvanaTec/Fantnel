using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V1200.Chat;

// ConnectionProtocol - 1.20.1
// PLAY.CLIENTBOUND
// ClientboundPlayerInfoUpdatePacket
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ClientBound, 58, EnumProtocolVersion.V1200)]
// ReSharper disable once UnusedType.Global
public class ClientboundPlayerInfoUpdatePacket : TabBase;