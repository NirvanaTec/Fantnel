using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Packet;

namespace NirvanaChat.Packet.V108X.Chat;

// ConnectionProtocol - 1.8.9
// PLAY.SERVERBOUND
// C01PacketChatMessage
[RegisterPacket(EnumConnectionState.Play, EnumPacketDirection.ServerBound, 1, EnumProtocolVersion.V108X)]
// ReSharper disable once UnusedType.Global
public class C01PacketChatMessage : CommandBase;