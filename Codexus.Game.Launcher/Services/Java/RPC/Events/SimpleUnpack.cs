using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codexus.Game.Launcher.Services.Java.RPC.Events;

public class SimpleUnpack(byte[] bytes) {
    private int _index;

    private ushort _lastLength;

    public void Unpack<T>(ref T content)
    {
        var fields = typeof(T).GetFields();
        foreach (var fieldInfo in fields) {
            var value = fieldInfo.GetValue(content);
            var fieldType = fieldInfo.FieldType;
            if (value == null) continue;
            InnerUnpack(ref value, fieldType);
            fieldInfo.SetValue(content, value);
        }

        var properties = typeof(T).GetProperties();
        foreach (var propertyInfo in properties)
            if (propertyInfo.CanWrite) {
                var value2 = propertyInfo.GetValue(content);
                var propertyType = propertyInfo.PropertyType;
                if (value2 == null) continue;
                InnerUnpack(ref value2, propertyType);
                propertyInfo.SetValue(content, value2);
            }

        if (content != null) content = ConvertValue<T>(content);
    }

    private static T ConvertValue<T>(object value)
    {
        return (T)Convert.ChangeType(value, typeof(T));
    }

    private void InnerUnpack(ref object value, Type type)
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (Type.GetTypeCode(type)) {
            case TypeCode.Object:
                if (type == typeof(byte[])) {
                    value = bytes.Skip(_index).Take(_lastLength).ToArray();
                    _index += _lastLength;
                } else if (type == typeof(List<uint>)) {
                    var num = _lastLength;
                    var list = new List<uint>();
                    while (num > 0) {
                        list.Add(BitConverter.ToUInt32(bytes, _index));
                        _index += 4;
                        num -= 4;
                    }

                    value = list;
                }

                break;
            case TypeCode.Byte:
                value = bytes[_index++];
                break;
            case TypeCode.Int16:
                value = BitConverter.ToInt16(bytes, _index);
                _index += 2;
                break;
            case TypeCode.UInt16:
                value = BitConverter.ToUInt16(bytes, _index);
                _index += 2;
                _lastLength = (ushort)value;
                break;
            case TypeCode.Int32:
                value = BitConverter.ToInt32(bytes, _index);
                _index += 4;
                break;
            case TypeCode.UInt32:
                value = BitConverter.ToUInt32(bytes, _index);
                _index += 4;
                break;
            case TypeCode.String:
                value = Encoding.UTF8.GetString(bytes, _index, _lastLength);
                _index += _lastLength;
                break;
        }
    }
}