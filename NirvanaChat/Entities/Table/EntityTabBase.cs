using DotNetty.Buffers;

namespace NirvanaChat.Entities.Table;

public class EntityTabBase {
    private readonly List<long> _uniqueIdList = [];
    public string? NewName = null;
    public string? OldName = null;

    public string Text = string.Empty;

    protected EntityTabBase(IByteBuffer buffer)
    {
        _uniqueIdList.Add(buffer.ReadLong()); // readUniqueId();
        _uniqueIdList.Add(buffer.ReadLong()); // readUniqueId();
    }

    protected void WriteToBuffer(IByteBuffer buffer)
    {
        foreach (var uniqueId in _uniqueIdList) {
            buffer.WriteLong(uniqueId);
        }
    }
}