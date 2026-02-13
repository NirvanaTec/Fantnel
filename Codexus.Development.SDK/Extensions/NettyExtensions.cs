using System.Text;
using Codexus.Development.SDK.Entities;
using Codexus.Development.SDK.Utils;
using DotNetty.Buffers;
using DotNetty.Common.Utilities;

namespace Codexus.Development.SDK.Extensions;

public static class NettyExtensions {
    public static int ReadVarInt(this byte[] buffer)
    {
        var num = 0;
        var num2 = 0;
        foreach (var b in buffer) {
            num |= (b & 0x7F) << (num2++ * 7);
            if (num2 > 5) {
                throw new Exception("VarInt too big");
            }

            if ((b & 0x80) != 128) {
                return num;
            }
        }

        throw new IndexOutOfRangeException();
    }

    public static Position ReadPosition(this IByteBuffer buffer)
    {
        var num = buffer.ReadLong();
        var x = (int)(num >> 38);
        var y = (int)((num << 52) >> 52);
        var z = (int)((num << 26) >> 38);
        return new Position(x, y, z);
    }

    public static int ReadVarIntFromBuffer(this IByteBuffer buffer)
    {
        var num = 0;
        var num2 = 0;
        while (true) {
            var b = buffer.ReadByte();
            num |= (b & 0x7F) << num2;
            if ((b & 0x80) == 0) {
                break;
            }

            num2 += 7;
            if (num2 >= 32) {
                throw new Exception("VarInt is too big");
            }
        }

        return num;
    }

    public static List<Property> ReadProperties(this IByteBuffer buffer)
    {
        var properties = new List<Property>();
        buffer.ReadWithCount(delegate {
            var item = buffer.ReadProperty();
            properties.Add(item);
        });
        return properties;
    }

    public static Property ReadProperty(this IByteBuffer buffer)
    {
        var name = buffer.ReadStringFromBuffer();
        var value = buffer.ReadStringFromBuffer();
        var signature = buffer.ReadNullable(() => buffer.ReadStringFromBuffer());
        return new Property {
            Name = name,
            Value = value,
            Signature = signature
        };
    }

    public static T? ReadNullable<T>(this IByteBuffer buffer, Func<T> action)
    {
        if (!buffer.ReadBoolean()) {
            return default;
        }

        return action();
    }

    public static void ReadWithCount(this IByteBuffer buffer, Action action)
    {
        var num = buffer.ReadVarIntFromBuffer();
        for (var i = 0; i < num; i++) {
            action();
        }
    }

    public static string ReadStringFromBuffer(this IByteBuffer buffer, int maxLength = 32767)
    {
        var num = buffer.ReadVarIntFromBuffer();
        if (num > maxLength * 4) {
            throw new Exception("The received encoded string buffer length is longer than maximum allowed (" + num +
                                " > " + maxLength * 4 + ")");
        }

        if (num < 0) {
            throw new Exception("The received encoded string buffer length is less than zero! Weird string!");
        }

        if (num > buffer.ReadableBytes) {
            num = buffer.ReadableBytes;
        }

        var array = new byte[num];
        buffer.ReadBytes(array);
        var text = Encoding.UTF8.GetString(array);
        if (text.Length > maxLength) {
            throw new Exception("The received string length is longer than maximum allowed (" + num + " > " +
                                maxLength + ")");
        }

        return text;
    }

    public static byte[] ReadByteArrayFromBuffer(this IByteBuffer buffer, int length)
    {
        var array = new byte[length];
        buffer.ReadBytes(array);
        return array;
    }

    public static byte[] ReadByteArrayFromBuffer(this IByteBuffer buffer)
    {
        var num = buffer.ReadVarIntFromBuffer();
        if (num < 0) {
            throw new Exception("The received encoded string buffer length is less than zero! Weird string!");
        }

        var array = new byte[num];
        buffer.ReadBytes(array);
        return array;
    }

    public static byte[] ReadByteArrayReadableBytes(this IByteBuffer buffer)
    {
        var array = new byte[buffer.ReadableBytes];
        buffer.ReadBytes(array);
        return array;
    }

    public static IByteBuffer WriteStringToBuffer(this IByteBuffer buffer, string stringToWrite, int maxLength = 32767)
    {
        if (stringToWrite.Length > maxLength) {
            throw new Exception(
                "String too big (was " + stringToWrite.Length + " bytes encoded, max " + maxLength + ")");
        }

        var bytes = Encoding.UTF8.GetBytes(stringToWrite);
        buffer.WriteVarInt(bytes.Length);
        buffer.WriteBytes(bytes);
        return buffer;
    }

    public static IByteBuffer WriteByteArrayToBuffer(this IByteBuffer buffer, byte[] bytes)
    {
        return buffer.WriteVarInt(bytes.Length).WriteBytes(bytes);
    }

    public static IByteBuffer WritePosition(this IByteBuffer buffer, int x, int y, int z)
    {
        return buffer.WritePosition(new Position(x, y, z));
    }

    public static IByteBuffer WritePosition(this IByteBuffer buffer, Position position)
    {
        var value = (long)((((ulong)position.X & 0x3FFFFFFuL) << 38) | (((ulong)position.Z & 0x3FFFFFFuL) << 12) |
                           ((ulong)position.Y & 0xFFFuL));
        return buffer.WriteLong(value);
    }

    public static IByteBuffer WriteProperties(this IByteBuffer buffer, List<Property> properties)
    {
        if (properties == null) {
            buffer.WriteVarInt(0);
            return buffer;
        }

        buffer.WriteWithCount(properties.Count, delegate(int index) { buffer.WriteProperty(properties[index]); });
        return buffer;
    }

    public static IByteBuffer WriteProperty(this IByteBuffer buffer, Property property)
    {
        buffer.WriteStringToBuffer(property.Name);
        buffer.WriteStringToBuffer(property.Value);
        buffer.WriteNullable(false, delegate { buffer.WriteStringToBuffer(property.Signature); });
        return buffer;
    }

    public static IByteBuffer WriteVarInt(this IByteBuffer buffer, int input)
    {
        while ((input & -128) != 0) {
            buffer.WriteByte((input & 0x7F) | 0x80);
            input >>>= 7;
        }

        buffer.WriteByte(input);
        return buffer;
    }

    public static void WriteWithCount(this IByteBuffer buffer, int count, Action<int> action)
    {
        buffer.WriteVarInt(count);
        for (var i = 0; i < count; i++) {
            action(i);
        }
    }

    public static void WriteNullable(this IByteBuffer buffer, bool nullable, Action action)
    {
        if (nullable) {
            buffer.WriteBoolean(false);
            return;
        }

        buffer.WriteBoolean(true);
        action();
    }

    public static int GetVarIntSize(this int input)
    {
        for (var i = 1; i < 5; i++) {
            if ((input & (-1 << (i * 7))) == 0) {
                return i;
            }
        }

        return 5;
    }

    public static T WithReaderScope<T>(this IByteBuffer buffer, Func<IByteBuffer, T> action)
    {
        buffer.MarkReaderIndex();
        try {
            return action(buffer);
        } finally {
            buffer.ResetReaderIndex();
        }
    }

    public static IByteBuffer WriteSignedByte(this IByteBuffer buffer, sbyte value)
    {
        buffer.WriteByte((byte)value);
        return buffer;
    }

    public static T GetOrDefault<T>(this IAttribute<T> attribute, Func<T> value)
    {
        if (attribute.Get() == null) {
            attribute.SetIfAbsent(value());
        }

        return attribute.Get();
    }
}