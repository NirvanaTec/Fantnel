using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;
using OpenSDK.Cipher;

namespace OpenSDK.Extensions;

public static class YggdrasilExtensions
{
    extension(byte[] input)
    {
        public byte[] EncodeSha256()
        {
            return SHA256.HashData(input);
        }
    }

    extension(int value)
    {
        public byte[] ToByteArray(bool littleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian != littleEndian) Array.Reverse(bytes);

            return bytes;
        }

        private byte[] ToShortByteArray(bool littleEndian = true)
        {
            var bytes = BitConverter.GetBytes((short)value);
            if (BitConverter.IsLittleEndian != littleEndian) Array.Reverse(bytes);

            return bytes;
        }
    }

    extension(long value)
    {
        private byte[] ToByteArray(bool littleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian != littleEndian) Array.Reverse(bytes);

            return bytes;
        }

        public byte[] ToShortByteArray(bool littleEndian = true)
        {
            var bytes = BitConverter.GetBytes((short)value);
            if (BitConverter.IsLittleEndian != littleEndian) Array.Reverse(bytes);

            return bytes;
        }
    }

    extension(ChaChaPacker packer)
    {
        public byte[] PackMessage(byte type, byte[] data)
        {
            var message = new byte[data.Length + 10];
            var length = BitConverter.GetBytes((short)(message.Length - 2));
            Array.Copy(length, 0, message, 0, 2);

            message[6] = type;
            message[7] = 136;
            message[8] = 136;
            message[9] = 136;
            Array.Copy(data, 0, message, 10, data.Length);

            var crc32 = Crc32.Hash(message.AsSpan(6));
            Array.Copy(crc32, 0, message, 2, 4);

            packer.ProcessBytes(message, 2, message.Length - 2, message, 2);
            return message;
        }

        public (byte, byte[]) UnpackMessage(byte[] data)
        {
            packer.ProcessBytes(data, 0, data.Length, data, 0);

            var crc32Data = new byte[4];
            Crc32.Hash(data.AsSpan(4, data.Length - 4), crc32Data);

            for (var i = 0; i < 4; i++)
                if (crc32Data[i] != data[i])
                    throw new Exception("Unpacking failed");

            var result = new byte[data.Length - 8];
            Array.Copy(data, 8, result, 0, result.Length);
            return (data[4], result);
        }
    }

    extension(MemoryStream stream)
    {
        public void WriteInt(int value, bool littleEndian = true)
        {
            var bytes = value.ToByteArray(littleEndian);
            stream.Write(bytes);
        }

        public void WriteShort(int value, bool littleEndian = true)
        {
            var bytes = value.ToShortByteArray(littleEndian);
            stream.Write(bytes);
        }

        public void WriteLong(long value, bool littleEndian = true)
        {
            var bytes = value.ToByteArray(littleEndian);
            stream.Write(bytes);
        }

        public void WriteBytes(byte[] data)
        {
            stream.Write(data);
        }

        public void WriteString(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            stream.WriteByte((byte)bytes.Length);
            stream.Write(bytes);
        }

        public void WriteByteLengthString(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            stream.WriteByte((byte)bytes.Length);
            stream.Write(bytes);
        }

        public void WriteShortString(string value, bool littleEndian = true)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            WriteShort(stream, bytes.Length, littleEndian);
            stream.Write(bytes);
        }

        public void WriteShortBytes(byte[] data, bool littleEndian = true)
        {
            WriteShort(stream, data.Length, littleEndian);
            stream.Write(data);
        }
    }
}