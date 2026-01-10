using System.Text.Json;
using WPFLauncherApi.Utils.CodeTools;

namespace WPFLauncherApi.Protocol;

public static class X19
{
    public const string Channel = "netease";

    // CRC盐值
    public static string? CrcSalt;

    // 最新盒子版本号
    public static string GameVersion => GetLatestVersion();

    public static string GetCrcSalt()
    {
        return CrcSalt ?? throw new ErrorCodeException(ErrorCode.CrcSaltNotSet);
    }

    /**
     * @return 最新盒子的补丁信息
     */
    private static async Task<Dictionary<string, object>?> GetPatchVersionsAsync()
    {
        var str = await new HttpClient().GetAsync("https://x19.update.netease.com/pl/x19_java_patchlist");
        var content = await str.Content.ReadAsStringAsync();
        content = content[..content.LastIndexOf(',')];
        content = content[(content.LastIndexOf('\n') + 1)..];
        content = "{" + content + "}";
        return JsonSerializer.Deserialize<Dictionary<string, object>>(content);
    }

    /**
     * @return 最新盒子版本号
     */
    private static string GetLatestVersion()
    {
        var result = GetPatchVersionsAsync().GetAwaiter().GetResult();
        return result?.Keys.Last() ?? throw new Exception("X19 versions is empty.");
    }
}