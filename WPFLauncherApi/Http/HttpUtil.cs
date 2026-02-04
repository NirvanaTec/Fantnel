using System.Security.Cryptography;
using System.Text;

namespace WPFLauncherApi.Http;

public static class HttpUtil {
    private const string SKeys =
        "MK6mipwmOUedplb6,OtEylfId6dyhrfdn,VNbhn5mvUaQaeOo9,bIEoQGQYjKd02U0J,fuaJrPwaH2cfXXLP,LEkdyiroouKQ4XN1,jM1h27H4UROu427W,DhReQada7gZybTDk,ZGXfpSTYUvcdKqdY,AZwKf7MWZrJpGR5W,amuvbcHw38TcSyPU,SI4QotspbjhyFdT0,VP4dhjKnDGlSJtbB,UXDZx4KhZywQ2tcn,NIK73ZNvNqzva4kd,WeiW7qU766Q1YQZI";

    private static Aes Aes {
        get {
            var tempAes = Aes.Create();
            tempAes.Padding = PaddingMode.None;

            return tempAes;
        }
    }

    private static byte[][] HttpKeys {
        get { return SKeys.Split(',').Select(keys => Encoding.GetEncoding("us-ascii").GetBytes(keys)).ToArray(); }
    }

    public static byte[] HttpEncrypt(byte[] bodyIn)
    {
        var body = new byte[(int)Math.Ceiling((double)(bodyIn.Length + 16) / 16) * 16];
        Array.Copy(bodyIn, body, bodyIn.Length);

        var initVector = Encoding.GetEncoding("us-ascii").GetBytes("szkgpbyimxavqjcn");

        for (var i = 0; i < initVector.Length; i++)
            body[i + bodyIn.Length] = initVector[i];

        var keyIndex = (byte)((Random.Shared.Next(0, HttpKeys.Length - 1) << 4) | 2);

        var encryptedData = Aes.CreateEncryptor(HttpKeys[(keyIndex >> 4) & 0xF], initVector)
            .TransformFinalBlock(body, 0, body.Length);

        var result = new byte[16 + encryptedData.Length + 1];

        Array.Copy(initVector, result, 16);
        Array.Copy(encryptedData, 0, result, 16, encryptedData.Length);
        result[^1] = keyIndex;

        return result;
    }

    public static byte[]? HttpDecrypt(byte[] body)
    {
        if (body.Length < 0x12)
            return null;

        var encryptedData = body.Skip(16).Take(body.Length - 1 - 16).ToArray();

        var decryptedData = Aes.CreateDecryptor(HttpKeys[(body[^1] >> 4) & 0xF], body.Take(16).ToArray())
            .TransformFinalBlock(encryptedData, 0, encryptedData.Length);

        var scissor = 0;
        var scissorPos = decryptedData.Length - 1;

        while (scissor < 16)
            if (decryptedData[scissorPos--] != 0x00)
                scissor++;

        return decryptedData.Take(scissorPos + 1).ToArray();
    }
}