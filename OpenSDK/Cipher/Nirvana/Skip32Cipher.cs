using System.Security.Cryptography;
using System.Text;

namespace OpenSDK.Cipher.Nirvana;

public class Skip32Cipher(byte[] key)
{
    private static readonly byte[] FTable =
    [
        163, 215, 9, 131, 248, 72, 246, 244, 179, 33,
        21, 120, 153, 177, 175, 249, 231, 45, 77, 138,
        206, 76, 202, 46, 82, 149, 217, 30, 78, 56,
        68, 40, 10, 223, 2, 160, 23, 241, 96, 104,
        18, 183, 122, 195, 233, 250, 61, 83, 150, 132,
        107, 186, 242, 99, 154, 25, 124, 174, 229, 245,
        247, 22, 106, 162, 57, 182, 123, 15, 193, 147,
        129, 27, 238, 180, 26, 234, 208, 145, 47, 184,
        85, 185, 218, 133, 63, 65, 191, 224, 90, 88,
        128, 95, 102, 11, 216, 144, 53, 213, 192, 167,
        51, 6, 101, 105, 69, 0, 148, 86, 109, 152,
        155, 118, 151, 252, 178, 194, 176, 254, 219, 32,
        225, 235, 214, 228, 221, 71, 74, 29, 66, 237,
        158, 110, 73, 60, 205, 67, 39, 210, 7, 212,
        222, 199, 103, 24, 137, 203, 48, 31, 141, 198,
        143, 170, 200, 116, 220, 201, 93, 92, 49, 164,
        112, 136, 97, 44, 159, 13, 43, 135, 80, 130,
        84, 100, 38, 125, 3, 64, 52, 75, 28, 115,
        209, 196, 253, 59, 204, 251, 127, 171, 230, 62,
        91, 165, 173, 4, 35, 156, 20, 81, 34, 240,
        41, 121, 113, 126, 255, 140, 14, 226, 12, 239,
        188, 114, 117, 111, 55, 161, 236, 211, 142, 98,
        139, 134, 16, 232, 8, 119, 17, 190, 146, 79,
        36, 197, 50, 54, 157, 207, 243, 166, 187, 172,
        94, 108, 169, 19, 87, 37, 181, 227, 189, 168,
        58, 1, 5, 89, 42, 70
    ];

    private readonly byte[] _key = key.Length == 10
        ? key
        : throw new ArgumentOutOfRangeException(nameof(key), "Key must be {10} bytes.");

    public uint Encrypt(uint value)
    {
        return (uint)Encrypt((int)value);
    }

    public int Encrypt(int value)
    {
        int[] buf =
        [
            (value >> 24) & byte.MaxValue,
            (value >> 16) & byte.MaxValue,
            (value >> 8) & byte.MaxValue,
            value & byte.MaxValue
        ];
        Skip32(buf, true);
        return (buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3];
    }

    private void Skip32(int[] buf, bool encrypt)
    {
        int num;
        int k1;
        if (encrypt)
        {
            num = 1;
            k1 = 0;
        }
        else
        {
            num = -1;
            k1 = 23;
        }

        var w1 = (buf[0] << 8) + buf[1];
        var w2 = (buf[2] << 8) + buf[3];
        for (var index = 0; index < 12; ++index)
        {
            w2 ^= G(_key, k1, w1) ^ k1;
            var k2 = k1 + num;
            w1 ^= G(_key, k2, w2) ^ k2;
            k1 = k2 + num;
        }

        buf[0] = w2 >> 8;
        buf[1] = w2 & byte.MaxValue;
        buf[2] = w1 >> 8;
        buf[3] = w1 & byte.MaxValue;
    }

    private static int G(byte[] key, int k, int w)
    {
        var num = w >> 8;
        var num2 = w & 0xFF;
        var num3 = FTable[num2 ^ (key[4 * k % 10] & 0xFF)] ^ num;
        var num4 = FTable[num3 ^ (key[(4 * k + 1) % 10] & 0xFF)] ^ num2;
        var num5 = FTable[num4 ^ (key[(4 * k + 2) % 10] & 0xFF)] ^ num3;
        var num6 = FTable[num5 ^ (key[(4 * k + 3) % 10] & 0xFF)] ^ num4;
        return (num5 << 8) + num6;
    }

    public string GenerateRoleUuid(string roleName, uint userId)
    {
        var numArray = MD5.HashData(Encoding.UTF8.GetBytes(roleName));
        var bytes = BitConverter.GetBytes(Encrypt(userId));
        Buffer.BlockCopy(bytes, 0, numArray, 12, bytes.Length);
        numArray[6] = (byte)((numArray[6] & 15) | 64);
        numArray[8] = (byte)((numArray[8] & 63) | 128);
        return Convert.ToHexStringLower(numArray);
    }

    public uint ComputeUserIdFromUuid(string uuid)
    {
        uuid = uuid.Replace("-", "");
        return uuid.Length != 32 ? 0U : Decrypt(BitConverter.ToUInt32(Convert.FromHexString(uuid), 12));
    }

    private uint Decrypt(uint value)
    {
        return (uint)Decrypt((int)value);
    }

    private int Decrypt(int value)
    {
        int[] buf =
        [
            (value >> 24) & byte.MaxValue,
            (value >> 16) & byte.MaxValue,
            (value >> 8) & byte.MaxValue,
            value & byte.MaxValue
        ];
        Skip32(buf, false);
        return (buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3];
    }
}