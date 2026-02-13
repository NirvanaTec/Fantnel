using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V1180.Chat;

// ConnectionProtocol - 1.18.1
// PLAY.ClientBound
// ClientboundPlayerInfoPacket
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ClientBound, 54, EnumProtocolVersion.V1180)]
// ReSharper disable once UnusedType.Global
public class ClientboundPlayerInfoPacket : TabBase;