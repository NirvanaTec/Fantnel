using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V108X.Chat;

// ConnectionProtocol - 1.8.9
// PLAY.ClientBound
// S38PacketPlayerListItem
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ClientBound, 56, EnumProtocolVersion.V108X)]
// ReSharper disable once UnusedType.Global
public class S38PacketPlayerListItem : TabBase;