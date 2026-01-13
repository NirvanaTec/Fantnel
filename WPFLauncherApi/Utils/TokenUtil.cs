using System.Text;
using WPFLauncherApi.Utils.CodeTools;

namespace WPFLauncherApi.Utils;

public static class TokenUtil
{
    public static Dictionary<string, string> Compute(
        string requestPath,
        string sendBody)
    {
        if (WPFLauncherProgram.User.UserId == null || WPFLauncherProgram.User.Token == null)
            throw new ErrorCodeException(ErrorCode.LogInNot);
        return Compute(requestPath, sendBody, WPFLauncherProgram.User.UserId, WPFLauncherProgram.User.Token);
    }

    public static Dictionary<string, string> Compute(
        string requestPath,
        string sendBody,
        string userId,
        string userToken)
    {
        return ComputeHttpRequestToken(requestPath, Encoding.UTF8.GetBytes(sendBody), userId, userToken);
    }

    private static Dictionary<string, string> ComputeHttpRequestToken(
        string requestPath,
        byte[] sendBody,
        string userId,
        string userToken)
    {
        requestPath = requestPath.StartsWith('/') ? requestPath : "/" + requestPath;
        using var memoryStream = new MemoryStream();
        memoryStream.Write((ReadOnlySpan<byte>)Encoding.UTF8.GetBytes(userToken.EncodeMd5().ToLowerInvariant()));
        memoryStream.Write((ReadOnlySpan<byte>)sendBody);
        memoryStream.Write("0eGsBkhl"u8);
        memoryStream.Write((ReadOnlySpan<byte>)Encoding.UTF8.GetBytes(requestPath));
        var lowerInvariant = memoryStream.ToArray().EncodeMd5().ToLowerInvariant();
        var binary = HexToBinary(lowerInvariant);
        var secretBin = binary[6..] + binary[..6];
        var bytes = Encoding.UTF8.GetBytes(lowerInvariant);
        ProcessBinaryBlock(secretBin, bytes);
        var str2 = (Convert.ToBase64String(bytes, 0, 12) + "1").Replace('+', 'm').Replace('/', 'o');
        return new Dictionary<string, string>
        {
            ["user-id"] = userId,
            ["user-token"] = str2
        };
    }

    private static void ProcessBinaryBlock(string secretBin, byte[] httpToken)
    {
        for (var index1 = 0; index1 < secretBin.Length / 8; ++index1)
        {
            var readOnlySpan = secretBin.AsSpan(index1 * 8, Math.Min(8, secretBin.Length - index1 * 8));
            byte num = 0;
            for (var index2 = 0; index2 < readOnlySpan.Length; ++index2)
                if (readOnlySpan[7 - index2] == '1')
                    num |= (byte)(1 << index2);
            httpToken[index1] ^= num;
        }
    }

    private static string HexToBinary(string hexString)
    {
        var stringBuilder = new StringBuilder();
        foreach (var str in hexString.Select((Func<char, string>)(hex => Convert.ToString(hex, 2).PadLeft(8, '0'))))
            stringBuilder.Append(str);
        return stringBuilder.ToString();
    }
}