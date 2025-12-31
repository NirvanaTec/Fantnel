using System.Security.Cryptography;
using System.Text;

namespace WPFLauncherApi.Utils;

public static class MPayExtensions
{
    public static string EncodeMd5(this string input)
    {
        return string.IsNullOrEmpty(input) ? string.Empty : Encoding.UTF8.GetBytes(input).EncodeMd5();
    }

    public static string EncodeMd5(this byte[] inputBytes)
    {
        return MD5.HashData(inputBytes).EncodeHex();
    }

    public static string EncodeBase64(this string input)
    {
        return string.IsNullOrEmpty(input) ? string.Empty : Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
    }

    private static string EncodeHex(this byte[] input)
    {
        return Convert.ToHexString(input).Replace("-", "").ToLower();
    }

    public static byte[] DecodeHex(this string input)
    {
        return Convert.FromHexString(input);
    }

    public static byte[] EncodeAes(this string input, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        using var encryptor = aes.CreateEncryptor();
        var bytes = Encoding.UTF8.GetBytes(input);
        return encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
    }
}