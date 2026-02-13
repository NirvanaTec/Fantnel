using Codexus.Development.SDK.Connection;
using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Extensions;
using Codexus.Development.SDK.Packet;
using DotNetty.Buffers;
using NirvanaAPI;
using NirvanaChat.Entities.Table;
using NirvanaChat.Message;

namespace NirvanaChat.Packet;

public class TabBase : IPacket {
    private readonly List<EntityTabBase> _list = [];
    private int _action;

    private byte[]? _rawBytes;

    public EnumProtocolVersion ClientProtocolVersion { get; set; }

    public void ReadFromBuffer(IByteBuffer buffer)
    {
        _rawBytes = new byte[buffer.ReadableBytes];
        buffer.GetBytes(buffer.ReaderIndex, _rawBytes);
        // readEnumValue(Action.class);
        _action = buffer.ReadVarIntFromBuffer();
        // ADD_PLAYER : 0
        // UPDATE_GAME_MODE : 1
        // UPDATE_LATENCY : 2
        // UPDATE_DISPLAY_NAME : 3
        // REMOVE_PLAYER : 4
        if (_action != 0 && _action != 3) {
            return;
        }

        var size = buffer.ReadVarIntFromBuffer(); // readVarInt();
        for (var i = 0; i < size; i++) {
            switch (_action) {
                case 0:
                    _list.Add(new EntityTabAdd(buffer));
                    break;
                case 3:
                    _list.Add(new EntityTabUpdate(buffer));
                    break;
            }
        }
    }

    public void WriteToBuffer(IByteBuffer buffer)
    {
        var useList = false;
        foreach (var entityTab in _list) {
            if (entityTab.NewName != null) {
                useList = true;
                break;
            }
        }

        if (!useList) {
            buffer.WriteBytes(_rawBytes);
            return;
        }

        buffer.WriteVarInt(_action);
        buffer.WriteVarInt(_list.Count);
        foreach (var entityTab in _list) {
            switch (entityTab) {
                case EntityTabAdd entityTabAdd:
                    entityTabAdd.WriteToBuffer(buffer);
                    break;
                case EntityTabUpdate entityTabUpdate:
                    entityTabUpdate.WriteToBuffer(buffer);
                    break;
            }
        }
    }

    public bool HandlePacket(GameConnection gameConnection)
    {
        if (_rawBytes == null || !NirvanaConfig.Config.ChatEnable || !NirvanaConfig.Config.ChatTarget) {
            return false;
        }

        foreach (var entity in ChatMessage.GetPlayers(gameConnection.GameId)) {
            foreach (var entityTab in _list) {
                if (entityTab.Text.Contains(entity.Value.NickName)) {
                    entityTab.NewName = NirvanaConfig.Config.ChatPrefix + entity.Value.NickName;
                    entityTab.OldName = entity.Value.NickName;
                } else if (entityTab is EntityTabAdd entityTabAdd) {
                    if (entityTabAdd.Name.Contains(entity.Value.NickName)) {
                        entityTab.NewName = NirvanaConfig.Config.ChatPrefix + entity.Value.NickName;
                        entityTab.OldName = entity.Value.NickName;
                    }
                }
            }
        }

        return false;
    }
}