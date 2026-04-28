using System.Text.Json;
using Nirvana.WPFLauncher.Entities.EntitiesPc4399;
using Nirvana.WPFLauncher.Http;

namespace Nirvana.WPFLauncher.Protocol;

public class MgbSdk(string gameId) : IDisposable {
    private readonly HttpWrapper _sdk = new("https://mgbsdk.matrix.netease.com");

    public void Dispose()
    {
        _sdk.Dispose();
        GC.SuppressFinalize(this);
    }

    public static string GenerateSAuth(string sdkUid, string sessionId, string channel, string platform, string userId = "", string timestamp = "")
    {
        var str = Guid.NewGuid().ToString("N");
        return JsonSerializer.Serialize(new EntityMgbSdkSAuthJson {
            AppChannel = channel,
            ClientLoginSn = str,
            DeviceId = str,
            GameId = "x19",
            LoginChannel = channel,
            SdkUid = sdkUid,
            SessionId = sessionId,
            Timestamp = timestamp,
            Platform = platform,
            SourcePlatform = platform,
            Udid = str,
            UserId = userId
        });
    }

    public async Task AuthSession(string cookie)
    {
        var httpResponseMessage = await _sdk.PostAsync($"/{gameId}/sdk/uni_sauth", cookie);
        var responseText = await httpResponseMessage.Content.ReadAsStringAsync();
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseText);
        if (dictionary == null) {
            throw new HttpRequestException("Response is empty");
        }

        if (!"200".Equals(dictionary["code"].ToString())) {
            throw new HttpRequestException(dictionary["status"].ToString());
        }
    }
}