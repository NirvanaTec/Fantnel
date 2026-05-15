using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;

namespace Nirvana.DevPlugin.Extensions;

public static class CryptExtensions {
    public static string ToSha1(this MemoryStream data)
    {
        using var sha = SHA1.Create();
        var array = sha.ComputeHash(data);
        Array.Reverse(array);
        var bigInteger = new BigInteger(array);
        if (bigInteger < 0L) {
            return "-" + (-bigInteger).ToString("x").TrimStart('0');
        }

        return bigInteger.ToString("x").TrimStart('0');
    }
}