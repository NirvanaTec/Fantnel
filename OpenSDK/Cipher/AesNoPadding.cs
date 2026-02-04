using System.Security.Cryptography;

namespace OpenSDK.Cipher;

public static class AesNoPadding {
    public static byte[] Encrypt(byte[] data, byte[] key, bool encryption = true)
    {
        using var aesNoPadding = Aes.Create();
        aesNoPadding.Key = key;
        aesNoPadding.Mode = CipherMode.ECB;
        aesNoPadding.Padding = PaddingMode.None;

        var transform = encryption
            ? aesNoPadding.CreateEncryptor()
            : aesNoPadding.CreateDecryptor();

        return transform.TransformFinalBlock(data, 0, data.Length);
    }
}