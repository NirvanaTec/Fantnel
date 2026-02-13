using System.Text.Json;
using Serilog;
using WPFLauncherApi.Entities.EntitiesWPFLauncher;
using WPFLauncherApi.Http;
using WPFLauncherApi.Utils;
using HttpRequestOptions = WPFLauncherApi.Http.HttpRequestOptions;

namespace WPFLauncherApi.Protocol;

public static class InterConn {
    private static readonly HttpWrapper Core = new("https://x19obtcore.nie.netease.com:8443",
        (Action<HttpRequestOptions>)(builder => builder.UserAgent(WPFLauncher.GetUserAgent())));

    public static async Task LoginStart()
    {
        Log.Debug("LoginStart response: {0}",
            await (await Core.PostAsync1("/interconn/web/game-play-v2/login-start", "{\"strict_mode\":true}",
                    "application/json",
                    (Action<HttpWrapperBuilder>)(builder =>
                        builder.AddHeader(TokenUtil.Compute(builder.Url, builder.Body)))))
                .Content
                .ReadAsStringAsync());
    }

    public static async Task GameStart(string gameId)
    {
        Log.Debug("GameStart response: {0}", await (await Core.PostAsync1("/interconn/web/game-play-v2/start",
                JsonSerializer.Serialize(new InterConnGameStart {
                    GameId = gameId,
                    ItemList = ["10000"]
                }), "application/json",
                (Action<HttpWrapperBuilder>)(builder =>
                    builder.AddHeader(TokenUtil.Compute(builder.Url, builder.Body))))
            .ConfigureAwait(false)).Content.ReadAsStringAsync());
    }
}