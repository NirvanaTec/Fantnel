using System.Security.Cryptography;
using System.Text;

namespace WPFLauncherApi.Utils.Cipher;

public static class RandomUtil
{
    public static string GetRandomString(int length, string? chars = null)
    {
        if (length <= 0)
            return string.Empty;
        if (string.IsNullOrEmpty(chars))
            chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghizklmnopqrstuvwxyz0123456789";
        var stringBuilder = new StringBuilder(length);
        var data = new byte[length];
        RandomNumberGenerator.Fill((Span<byte>)data);
        for (var index1 = 0; index1 < length; ++index1)
        {
            var index2 = data[index1] % chars.Length;
            stringBuilder.Append(chars[index2]);
        }

        return stringBuilder.ToString();
    }

    public static string GenerateSessionId()
    {
        return "captchaReq" + GetRandomString(16);
    }
}