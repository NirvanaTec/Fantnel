using System.Text.Encodings.Web;
using System.Text.Json;
using WPFLauncherApi.Entities.EntitiesMgbSdk;
using WPFLauncherApi.Http;

namespace WPFLauncherApi.Protocol;

public class MgbSdk(string gameId) : IDisposable
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly HttpWrapper _sdk = new("https://mgbsdk.matrix.netease.com");

    public void Dispose()
    {
        _sdk.Dispose();
        GC.SuppressFinalize(this);
    }

    public string GenerateSAuth(
        string deviceId,
        string userid,
        string sdkUid,
        string sessionId,
        string timestamp,
        string channel,
        string platform = "pc")
    {
        var upper = sessionId.ToUpper();
        return JsonSerializer.Serialize(new EntityMgbSdkCookie
        {
            AppChannel = channel,
            ClientLoginSn = deviceId.ToUpper(),
            DeviceId = deviceId.ToUpper(),
            GameId = gameId,
            LoginChannel = channel,
            SdkUid = sdkUid,
            SessionId = upper,
            Timestamp = timestamp,
            Platform = platform,
            SourcePlatform = platform,
            Udid = deviceId.ToUpper(),
            UserId = userid
        }, DefaultOptions);
    }

    public async Task AuthSession(string cookie)
    {
        var httpResponseMessage = await _sdk.PostAsync($"/{gameId}/sdk/uni_sauth", cookie);
        if (!httpResponseMessage.IsSuccessStatusCode) throw new HttpRequestException(httpResponseMessage.ReasonPhrase);
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(await httpResponseMessage.Content
            .ReadAsStringAsync());
        if (dictionary == null) throw new HttpRequestException("Response is empty");
        if (!"200".Equals(dictionary["code"].ToString()))
            throw new HttpRequestException("Status: " + dictionary["status"]);
    }
}