using System.Text.Json;
using Codexus.Development.SDK.Extensions;
using DotNetty.Buffers;
using NirvanaAPI.Entities.EntitiesNirvana;

namespace NirvanaChat.Entities.Table;

public class EntityTabAdd : EntityTabBase {
    private readonly byte[] _bytes; // 剩余的
    private readonly bool _displayName;
    private readonly int _gameModeId;
    private readonly int _ping;
    private readonly List<EntityTabTarget> _targetList = [];
    public readonly string Name; // 可能是名称或文本

    public EntityTabAdd(IByteBuffer buffer) : base(buffer)
    {
        Name = buffer.ReadStringFromBuffer();
        var size = buffer.ReadVarIntFromBuffer(); // readVarInt();
        for (var i = 0; i < size; i++) {
            _targetList.Add(new EntityTabTarget(buffer));
        }

        _gameModeId = buffer.ReadVarIntFromBuffer(); // getGameMode().getID()
        _ping = buffer.ReadVarIntFromBuffer(); // getPing();
        _displayName = buffer.ReadBoolean();
        if (_displayName) {
            Text = buffer.ReadStringFromBuffer();
        }

        _bytes = buffer.ReadByteArrayReadableBytes();
    }

    public new void WriteToBuffer(IByteBuffer buffer)
    {
        base.WriteToBuffer(buffer);
        buffer.WriteStringToBuffer(Name);
        buffer.WriteVarInt(_targetList.Count);
        foreach (var target in _targetList) {
            target.WriteToBuffer(buffer);
        }

        buffer.WriteVarInt(_gameModeId);
        buffer.WriteVarInt(_ping);

        if (OldName != null && NewName != null) {
            buffer.WriteBoolean(true);
            var newText = Text;
            if (string.IsNullOrEmpty(newText)) {
                var entityText = new EntityText {
                    Text = NewName
                };
                newText = JsonSerializer.Serialize(entityText);
            } else {
                newText = newText.Replace(OldName, NewName);
            }

            buffer.WriteStringToBuffer(newText);
        } else {
            buffer.WriteBoolean(_displayName);
        }

        buffer.WriteBytes(_bytes);
    }
}