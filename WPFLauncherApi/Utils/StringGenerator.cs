namespace WPFLauncherApi.Utils;

public static class StringGenerator
{
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

        return string.Join(separator,
            mac[0].ToString(format),
            mac[1].ToString(format),
            mac[2].ToString(format),
            mac[3].ToString(format),
            mac[4].ToString(format),
            mac[5].ToString(format));
    }
}