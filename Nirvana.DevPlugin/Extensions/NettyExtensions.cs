using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using Nirvana.DevPlugin.Entities;
using Nirvana.DevPlugin.Utils;

namespace Nirvana.DevPlugin.Extensions;

public static class NettyExtensions {
    
    public static int ReadVarInt(this byte[] buffer)
    {
        var num = 0;
        var num2 = 0;
        foreach (var t in buffer) {
            var b = (sbyte)t;
            num |= (t & 0x7F) << (num2++ * 7);
            if (num2 > 5) {
                throw new Exception("VarInt too big");
            }

            if ((b & 0x80) != 128) {
                return num;
            }
        }

        throw new IndexOutOfRangeException();
    }

    extension(IByteBuffer buffer) {
        public Position ReadPosition()
        {
            var num = buffer.ReadLong();
            var x = (int)(num >> 38);
            var y = (int)((num << 52) >> 52);
            var z = (int)((num << 26) >> 38);
            return new Position(x, y, z);
        }

        public int ReadVarIntFromBuffer()
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

        public List<Property> ReadProperties()
        {
            var properties = new List<Property>();
            buffer.ReadWithCount(() => {
                var item = buffer.ReadProperty();
                properties.Add(item);
            });
            return properties;
        }

        public Property ReadProperty()
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

        public T? ReadNullable<T>(Func<T> action)
        {
            return !buffer.ReadBoolean() ? default : action();
        }

        public void ReadWithCount(Action action)
        {
            var num = buffer.ReadVarIntFromBuffer();
            for (var i = 0; i < num; i++) {
                action();
            }
        }

        public string ReadStringFromBuffer(int maxLength = 32767)
        {
            var num = buffer.ReadVarIntFromBuffer();
            if (num > maxLength * 4) {
                throw new Exception("The received encoded string buffer length is longer than maximum allowed (" + num + " > " + maxLength * 4 + ")");
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
            return text.Length > maxLength ? throw new Exception("The received string length is longer than maximum allowed (" + num + " > " + maxLength + ")") : text;
        }

        public byte[] ReadByteArrayFromBuffer(int length)
        {
            var array = new byte[length];
            buffer.ReadBytes(array);
            return array;
        }

        public byte[] ReadByteArrayFromBuffer()
        {
            var num = buffer.ReadVarIntFromBuffer();
            if (num < 0) {
                throw new Exception("The received encoded string buffer length is less than zero! Weird string!");
            }

            var array = new byte[num];
            buffer.ReadBytes(array);
            return array;
        }

        public byte[] ReadBytes()
        {
            var array = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(array);
            return array;
        }
    }

    public static List<string> ReadToRegistry(this byte[] bytes)
    {
        var context = Encoding.UTF8.GetString(bytes);
        return context.ReadToRegistry();
    }
    
    public static List<string> ReadToRegistry(this string context)
    {
        return context.Split('\0').ToList();
    }

    extension(IByteBuffer buffer) {
        public List<string> ReadToRegistry()
        {
            var context = buffer.ReadString();
            return context.ReadToRegistry();
        }

        public string ReadString()
        {
            return Encoding.UTF8.GetString(buffer.ReadBytes());
        }

        public IByteBuffer WriteStringToBuffer(string stringToWrite, int maxLength = 32767)
        {
            if (stringToWrite.Length > maxLength) {
                throw new Exception("String too big (was " + stringToWrite.Length + " bytes encoded, max " + maxLength + ")");
            }

            var bytes = Encoding.UTF8.GetBytes(stringToWrite);
            buffer.WriteVarInt(bytes.Length);
            buffer.WriteBytes(bytes);
            return buffer;
        }

        public IByteBuffer WriteByteArrayToBuffer(byte[] bytes)
        {
            buffer.WriteVarInt(bytes.Length);
            buffer.WriteBytes(bytes);
            return buffer;
        }

        public IByteBuffer WritePosition(int x, int y, int z)
        {
            return buffer.WritePosition(new Position(x, y, z));
        }

        public IByteBuffer WritePosition(Position position)
        {
            var value = (long)((((ulong)position.X & 0x3FFFFFFuL) << 38) | (((ulong)position.Z & 0x3FFFFFFuL) << 12) | ((ulong)position.Y & 0xFFFuL));
            return buffer.WriteLong(value);
        }

        public IByteBuffer WriteProperties(List<Property>? properties)
        {
            if (properties == null) {
                buffer.WriteVarInt(0);
                return buffer;
            }

            buffer.WriteWithCount(properties.Count, index => { buffer.WriteProperty(properties[index]); });
            return buffer;
        }

        public IByteBuffer WriteProperty(Property property)
        {
            buffer.WriteStringToBuffer(property.Name);
            buffer.WriteStringToBuffer(property.Value);
            buffer.WriteNullable(property.Signature == null, () => { buffer.WriteStringToBuffer(property.Signature); });
            return buffer;
        }

        public IByteBuffer WriteVarInt(int input)
        {
            while ((input & -128) != 0) {
                buffer.WriteByte((input & 0x7F) | 0x80);
                input >>>= 7;
            }

            buffer.WriteByte(input);
            return buffer;
        }

        public void WriteWithCount(int count, Action<int> action)
        {
            buffer.WriteVarInt(count);
            for (var i = 0; i < count; i++) {
                action(i);
            }
        }

        public void WriteNullable(bool nullable, Action action)
        {
            if (nullable) {
                buffer.WriteBoolean(false);
                return;
            }

            buffer.WriteBoolean(true);
            action();
        }
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

    extension(IByteBuffer buffer) {
        public T WithReaderScope<T>(Func<IByteBuffer, T> action)
        {
            buffer.MarkReaderIndex();
            try {
                return action(buffer);
            } finally {
                buffer.ResetReaderIndex();
            }
        }

        public IByteBuffer WriteSignedByte(sbyte value)
        {
            buffer.WriteByte((byte)value);
            return buffer;
        }
    }

    public static T GetOrDefault<T>(this IAttribute<T> attribute, Func<T> value)
    {
        if (attribute.Get() == null) {
            attribute.SetIfAbsent(value());
        }

        return attribute.Get();
    }
}