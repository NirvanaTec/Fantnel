using System.Security.Cryptography;
using System.Text;

namespace WPFLauncherApi.Utils.Cipher;

public static class TokenUtil {
    private static readonly Aes Aes;

    static TokenUtil()
    {
        Aes = Aes.Create();
        Aes.Mode = CipherMode.CBC;
        Aes.Padding = PaddingMode.Zeros;
        Aes.KeySize = 128;
        Aes.BlockSize = 128;
        Aes.Key = "debbde3548928fab"u8.ToArray();
        Aes.IV = "afd4c5c5a7c456a1"u8.ToArray();
    }

    public static string GenerateEncryptToken(string userToken)
    {
        var upper1 = RandomUtil.GetRandomString(8)
            .ToUpper();
        var upper2 = RandomUtil.GetRandomString(8)
            .ToUpper();
        var bytes = Encoding.ASCII.GetBytes(upper1 + userToken + upper2);
        return Convert.ToHexString(Aes.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length)).ToUpper();
    }
}