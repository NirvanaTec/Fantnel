using Codexus.Development.SDK.Extensions;
using DotNetty.Buffers;

namespace NirvanaChat.Entities.Table;

public class EntityTabTarget {
    private readonly bool _hasSignature;

    private readonly string _name;
    private readonly string? _signature;
    private readonly string _value;

    public EntityTabTarget(IByteBuffer buffer)
    {
        _name = buffer.ReadStringFromBuffer();
        _value = buffer.ReadStringFromBuffer();
        _hasSignature = buffer.ReadBoolean();
        if (_hasSignature) {
            _signature = buffer.ReadStringFromBuffer();
        }
    }

    public void WriteToBuffer(IByteBuffer buffer)
    {
        buffer.WriteStringToBuffer(_name);
        buffer.WriteStringToBuffer(_value);
        buffer.WriteBoolean(_hasSignature);
        if (_hasSignature) {
            buffer.WriteStringToBuffer(_signature ?? string.Empty);
        }
    }
}