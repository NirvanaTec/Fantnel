using System;
using System.Collections.Generic;
using System.Linq;
using Nirvana.Development.Packet.Base1200.Play.Server.Configuration;
using Nirvana.Development.Packet.Configuration.Client;
using Nirvana.Development.Packet.Configuration.Server;
using Nirvana.Development.Packet.Handshake.Client;
using Nirvana.Development.Packet.Login.Client;
using Nirvana.Development.Packet.Login.Server;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Packet;

namespace Nirvana.Development.Manager;

public static class PacketManager {
    public static readonly Dictionary<IPacket, RegisterPacket> BasePackets = new() {
        { new SPacketPluginMessage(), SPacketPluginMessage.RegisterPacket },

        { new CAcknowledgeConfiguration(), CAcknowledgeConfiguration.RegisterPacket },
        { new CAcknowledgeFinishConfiguration(), CAcknowledgeFinishConfiguration.RegisterPacket },

        { new SFinishConfiguration(), SFinishConfiguration.RegisterPacket },
        { new SStartConfiguration(), SStartConfiguration.RegisterPacket },

        { new CHandshake(), CHandshake.RegisterPacket },

        { new CPacketEncryptionResponse(), CPacketEncryptionResponse.RegisterPacket },
        { new CPacketLoginAcknowledged(), CPacketLoginAcknowledged.RegisterPacket },
        { new CPacketLoginStart(), CPacketLoginStart.RegisterPacket },

        { new SPacketDisconnect(), SPacketDisconnect.RegisterPacket },
        { new SPacketEnableCompression(), SPacketEnableCompression.RegisterPacket },
        { new SPacketEncryptionRequest(), SPacketEncryptionRequest.RegisterPacket },
        { new SPacketLoginSuccess(), SPacketLoginSuccess.RegisterPacket }
    };

    public static void TriggerEvent(Action<IPacket> onEvent, EnumConnectionState state, EnumPacketDirection direction, int packetId, EnumProtocolVersion protocolVersion, string? gameId = null)
    {
        var events = TriggerEvent(state, direction, packetId, protocolVersion, gameId);
        foreach (var item in events) {
            onEvent(item);
        }
    }

    public static IPacket? TriggerEvent(Func<IPacket, bool> onEvent, EnumConnectionState state, EnumPacketDirection direction, int packetId, EnumProtocolVersion protocolVersion, string? gameId = null)
    {
        var events = TriggerEvent(state, direction, packetId, protocolVersion, gameId);
        return events.FirstOrDefault(onEvent);
    }

    /**
    * 创建插件属性 和 内置属性
    */
    private static Dictionary<RegisterPacket, IPacket> CreateAttribute()
    {
        var packets = BasePackets.ToDictionary(x => x.Value, x => x.Key).ToDictionary(item => item.Key, item => item.Value);
        foreach (var item in PluginManager.CreateAttribute<RegisterPacket, IPacket>()) {
            packets.Add(item.Key, item.Value);
        }

        return packets;
    }

    private static IPacket[] TriggerEvent(EnumConnectionState state, EnumPacketDirection direction, int packetId, EnumProtocolVersion protocolVersion, string? gameId = null)
    {
        var list = new List<IPacket>();
        foreach (var item in CreateAttribute()) {
            var flag = true;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var version in item.Key.Versions) {
                if (version == EnumProtocolVersion.All || version == protocolVersion) {
                    flag = false;
                    break;
                }
            }

            if (flag) {
                continue;
            }

            if (item.Key.PacketId == packetId || item.Key.PacketId == -1) {
                if (item.Key.Direction == direction && item.Key.State == state) {
                    if (item.Key.GameId == gameId || item.Key.GameId == null) {
                        list.Add(item.Value);
                    }
                }
            }


        }

        return list.ToArray();
    }
}