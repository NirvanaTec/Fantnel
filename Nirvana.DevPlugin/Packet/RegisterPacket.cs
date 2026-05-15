using System;
using Nirvana.DevPlugin.Enums;

namespace Nirvana.DevPlugin.Packet;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterPacket(EnumConnectionState state, EnumPacketDirection direction, int packetId, string? gameId, params EnumProtocolVersion[] versions) : Attribute {
    
    public RegisterPacket(EnumConnectionState state, EnumPacketDirection direction, int packetId, params EnumProtocolVersion[] versions) : this(state, direction, packetId, null, versions)
    {
        
    }
    
    public RegisterPacket(EnumConnectionState state, EnumPacketDirection direction, int packetId, string? gameId = null) : this(state, direction, packetId, gameId, EnumProtocolVersion.All)
    {
        
    }
    
    // 为 null 时不限制游戏ID
    public readonly string? GameId = gameId;
    
    // -1 时获取所有包
    public readonly int PacketId = packetId;
    
    public readonly EnumConnectionState State = state;

    public readonly EnumPacketDirection Direction = direction;
    
    public readonly EnumProtocolVersion[] Versions = versions;
    
}