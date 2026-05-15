using System;
using System.Text;

namespace Nirvana.WPFLauncher.Utils;

public static class StringGenerator {
    private static readonly Random Random = new();

    public static string GenerateHexString(int length)
    {
        var numArray = new byte[length];
        Random.NextBytes(numArray);
        return Convert.ToHexString(numArray);
    }

    public static string GenerateRandomMacAddress(string separator = ":", bool uppercase = true)
    {
        var mac = new byte[6];
        Random.NextBytes(mac);

        mac[0] = (byte)(mac[0] & 0xFE);
        mac[0] = (byte)(mac[0] | 0x02);

        var format = uppercase ? "X2" : "x2";

        return string.Join(separator, mac[0].ToString(format), mac[1].ToString(format), mac[2].ToString(format), mac[3].ToString(format), mac[4].ToString(format), mac[5].ToString(format));
    }

    public static string GenerateRandomString(int length, bool includeNumbers = true, bool includeUppercase = true, bool includeLowercase = true)
    {
        if (length <= 0) {
            throw new ArgumentException("Length must be greater than 0", nameof(length));
        }

        if (!includeNumbers && !includeUppercase && !includeLowercase) {
            throw new ArgumentException("Must include at least one character type", nameof(length));
        }

        var stringBuilder = new StringBuilder();
        if (includeNumbers) {
            stringBuilder.Append("0123456789");
        }

        if (includeUppercase) {
            stringBuilder.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        }

        if (includeLowercase) {
            stringBuilder.Append("abcdefghijklmnopqrstuvwxyz");
        }

        var length2 = stringBuilder.Length;
        var stringBuilder2 = new StringBuilder(length);
        for (var i = 0; i < length; i++) {
            stringBuilder2.Append(stringBuilder[Random.Next(length2)]);
        }

        return stringBuilder2.ToString();
    }
}