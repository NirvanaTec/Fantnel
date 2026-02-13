using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V1122.Chat;

// ConnectionProtocol - 1.12.2
// PLAY.SERVERBOUND
// CPacketChatMessage
// 与 Base1122 冲突
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ServerBound, 2, EnumProtocolVersion.V1122)]
// ReSharper disable once UnusedType.Global
public class CPacketChatMessage : CommandBase;