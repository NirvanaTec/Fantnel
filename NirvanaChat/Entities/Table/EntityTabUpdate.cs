using Codexus.Development.SDK.Extensions;
using DotNetty.Buffers;

namespace NirvanaChat.Entities.Table;

public class EntityTabUpdate : EntityTabBase {
    private readonly byte[] _bytes; // 剩余的
    private readonly bool _displayName;

    public EntityTabUpdate(IByteBuffer buffer) : base(buffer)
    {
        _displayName = buffer.ReadBoolean();
        if (_displayName) {
            Text = buffer.ReadStringFromBuffer();
        }

        // Log.Warning("_nameBytes1: {_nameBytes}", _nameBytes);
        _bytes = buffer.ReadByteArrayReadableBytes();
    }

    public new void WriteToBuffer(IByteBuffer buffer)
    {
        base.WriteToBuffer(buffer);
        buffer.WriteBoolean(_displayName);
        if (_displayName) {
            var newText = Text;
            if (OldName != null && NewName != null) {
                newText = newText.Replace(OldName, NewName);
            }

            buffer.WriteStringToBuffer(newText);
        }

        buffer.WriteBytes(_bytes);
    }
}