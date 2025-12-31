using System.Text.Json;
using WPFLauncherApi.Http;
using WPFLauncherApi.Utils;
using HttpRequestOptions = WPFLauncherApi.Http.HttpRequestOptions;

namespace WPFLauncherApi.Protocol;

public static class X19
{
    private static readonly HttpWrapper ApiGateway = new("https://x19apigatewayobt.nie.netease.com",
        delegate(HttpRequestOptions options) { options.UserAgent("WPFLauncher/0.0.0.0"); });

    // 最新盒子版本号
    public static string GameVersion => GetLatestVersion();

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


    public static async Task<HttpResponseMessage> PostAsync(string url, string body)
    {
        return await ApiGateway.PostAsync(url, body, "application/json",
            delegate(HttpRequestOptions options) { options.AddHeaders(TokenUtil.Compute(url, body)); });
    }
}