using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V1122.Chat;

// ConnectionProtocol - 1.12.2
// PLAY.ClientBound
// ServerboundChatPacket
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ClientBound, 46, EnumProtocolVersion.V1122)]
// ReSharper disable once UnusedType.Global
public class SPacketPlayerListItem : TabBase;