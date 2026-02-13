using System.Numerics;
using System.Security.Cryptography;

namespace Codexus.Development.SDK.Extensions;

public static class CryptExtensions {
    public static string ToSha1(this MemoryStream data)
    {
        using var sHa = SHA1.Create();
        var array = sHa.ComputeHash(data);
        Array.Reverse(array);
        var bigInteger = new BigInteger(array);
        if (bigInteger < 0L) {
            return "-" + (-bigInteger).ToString("x").TrimStart('0');
        }

        return bigInteger.ToString("x").TrimStart('0');
    }
}