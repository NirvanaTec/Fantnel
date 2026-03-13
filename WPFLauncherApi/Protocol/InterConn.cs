using System.Text.Json;
using Serilog;
using WPFLauncherApi.Entities.EntitiesWPFLauncher;
using WPFLauncherApi.Http;
using WPFLauncherApi.Utils;
using HttpRequestOptions = WPFLauncherApi.Http.HttpRequestOptions;

namespace WPFLauncherApi.Protocol;

public static class InterConn {
    
    private static async Task LoginStart()
    {
        Log.Debug("LoginStart response: {0}", await X19Extensions.Core1.Api<string>("/interconn/web/game-play-v2/login-start", "{\"strict_mode\":true}"));
    }

    private static async Task GameStart(string gameId)
    {
        Log.Debug("GameStart response: {0}", await X19Extensions.Core1.Api<string>("/interconn/web/game-play-v2/start", new InterConnGameStart {
            GameId = gameId,
            ItemList = ["10000"]
        }));
    }

    public static async Task LoginStartAndGameStart(string gameId)
    {
        await LoginStart();
        await GameStart(gameId);
    }

}