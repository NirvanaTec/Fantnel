using System.Net;
using System.Text;
using System.Text.Json;
using WPFLauncherApi.Entities.EntitiesPc4399;
using QueryBuilder = WPFLauncherApi.Utils.QueryBuilder;

namespace WPFLauncherApi.Protocol;

public static class N4399 {
    public static string LoginWithPasswordAsync(
        string username,
        string password,
        string sessionId,
        string captcha)
    {
        // 构建登录参数
        var parameters = BuildLoginParameters();
        parameters.Add("username", username);
        parameters.Add("password", password);
        parameters.Add("sessionId", sessionId);
        parameters.Add("inputCaptcha", captcha);

        var client = new HttpClient(new HttpClientHandler {
            UseCookies = true,
            CookieContainer = new CookieContainer()
        });

        // 执行登录请求
        var loginResponse = client.PostAsync("https://ptlogin.4399.com/ptlogin/login.do?v=1",
            new FormUrlEncodedContent(parameters.GetAll()));

        if (!loginResponse.Result.IsSuccessStatusCode) {
            throw new Exception("登录请求失败");
        }

        var loginText = loginResponse.Result.Content.ReadAsStringAsync().Result;

        // 找到错误信息
        var errText = ExtractErrorTip(loginText);
        if (errText.Length > 0) {
            throw new Exception(errText);
        }

        var cookieString = string.Join("; ", loginResponse.Result.Headers.GetValues("Set-Cookie")
            .Select(cookie => cookie.Split(';')[0].Trim())
            .ToArray());

        // 生成SAuth令牌
        var sAuthToken = GenerateSAuthAsync(client, cookieString);
        return sAuthToken.Result;
    }

    private static string ExtractErrorTip(string html)
    {
        const string startMarker = "login_err_tip\">";
        const string endMarker = "</div>";

        var startIndex = html.IndexOf(startMarker, StringComparison.Ordinal);
        if (startIndex == -1) return string.Empty;

        startIndex += startMarker.Length;
        var endIndex = html.IndexOf(endMarker, startIndex, StringComparison.Ordinal);

        if (endIndex == -1) return string.Empty;

        // 提取内容并删除前后空格
        var content = html.Substring(startIndex, endIndex - startIndex);
        return content.Trim();
    }

    private static async Task<string> GenerateSAuthAsync(HttpClient client, string cookieString)
    {
        var unixTimeSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // 检查登录状态
        var checkUrl = "https://ptlogin.4399.com/ptlogin/checkKidLoginUserCookie.do?" +
                       "appId=kid_wdsj&gameUrl=http://cdn.h5wan.4399sj.com/microterminal-h5-frame?" +
                       "game_id=500352&rand_time=" + unixTimeSeconds + "&nick=null&onLineStart=false&" +
                       "show=1&isCrossDomain=1&retUrl=http%253A%252F%252Fptlogin.4399.com" +
                       "%252Fresource%252Fucenter.html%253Faction%253Dlogin%2526appId%253Dkid_wdsj%2526" +
                       "loginLevel%253D8%2526regLevel%253D8%2526bizId%253D2100001792%2526externalLogin%253D" +
                       "qq%2526qrLogin%253Dtrue%2526layout%253Dvertical%2526level%253D101%2526" +
                       "css%253Dhttp%253A%252F%252Fmicrogame.5054399.net%252Fv2%252Fresource%252F" +
                       "cssSdk%252Fdefault%252Flogin.css%2526v%253D2018_11_26_16%2526" +
                       "postLoginHandler%253Dredirect%2526checkLoginUserCookie%253Dtrue%2526" +
                       "redirectUrl%253Dhttp%25253A%25252F%25252Fcdn.h5wan.4399sj.com%25252F" +
                       "microterminal-h5-frame%25253Fgame_id%25253D500352%252526rand_time%25253D" + unixTimeSeconds;


        HttpResponseMessage checkResponse;
        while (true) {
            var request = new HttpRequestMessage(HttpMethod.Get, checkUrl);
            request.Headers.Add("Cookie", cookieString);
            checkResponse = await client.SendAsync(request);
            if (checkResponse.Headers.Location == null) break;
            checkUrl = checkResponse.Headers.Location.ToString();
        }

        if (checkResponse.RequestMessage?.RequestUri == null) {
            throw new Exception("登录状态检查失败");
        }

        var redirectUri = checkResponse.RequestMessage.RequestUri.ToString();
        var queryParams = QueryBuilder.FromParameters(redirectUri);

        // 获取统一认证信息
        var uniAuth = await GetUniAuthAsync(queryParams, client);

        // 生成SAuth令牌
        return GenerateSAuth(
            uniAuth.Get("username"),
            uniAuth.Get("uid"),
            uniAuth.Get("token"),
            uniAuth.Get("time"),
            "4399pc"
        );
    }

    private static string GenerateSAuth(
        string userId,
        string sdkUid,
        string sessionId,
        string timestamp,
        string channel,
        string platform = "pc")
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

    private static async Task<QueryBuilder> GetUniAuthAsync(QueryBuilder queryParams, HttpClient client)
    {
        var sdkUrl = new StringBuilder("https://microgame.5054399.net/v2/service/sdk/info?")
            .Append("callback=&queryStr=game_id%3D500352%26nick%3Dnull%26sig%3D")
            .Append(queryParams.Get("sig"))
            .Append("%26uid%3D")
            .Append(queryParams.Get("uid"))
            .Append("%26fcm%3D0%26show%3D1%26isCrossDomain%3D1%26rand_time%3D")
            .Append(queryParams.Get("rand_time"))
            .Append("%26ptusertype%3D4399%26time%3D")
            .Append(queryParams.Get("time"))
            .Append("%26validateState%3D")
            .Append(queryParams.Get("validateState"))
            .Append("%26username%3D")
            .Append(queryParams.Get("username"))
            .Append("&_=")
            .Append(queryParams.Get("time"));

        var response = await client.GetAsync(sdkUrl.ToString());

        if (!response.IsSuccessStatusCode) {
            throw new Exception("获取统一认证信息失败");
        }

        var responseText = await response.Content.ReadAsStringAsync();
        var uniAuthData = JsonSerializer.Deserialize<EntityC4399UniAuth>(responseText) ??
                          throw new Exception("解析统一认证数据失败");

        return new QueryBuilder(uniAuthData.Data.SdkLoginData);
    }

    private static QueryBuilder BuildLoginParameters()
    {
        var queryBuilder = new QueryBuilder();
        queryBuilder.Add("loginFrom", "uframe");
        queryBuilder.Add("postLoginHandler", "default");
        queryBuilder.Add("layoutSelfAdapting", "true");
        queryBuilder.Add("externalLogin", "qq");
        queryBuilder.Add("displayMode", "popup");
        queryBuilder.Add("layout", "vertical");
        queryBuilder.Add("bizId", "2100001792");
        queryBuilder.Add("appId", "kid_wdsj");
        queryBuilder.Add("gameId", "wd");
        queryBuilder.Add("css", "https://microgame.5054399.net/v2/resource/cssSdk/default/login.css");
        queryBuilder.Add("redirectUrl", "");
        queryBuilder.Add("mainDivId", "popup_login_div");
        queryBuilder.Add("includeFcmInfo", "false");
        queryBuilder.Add("level", "8");
        queryBuilder.Add("regLevel", "8");
        queryBuilder.Add("userNameLabel", "4399用户名");
        queryBuilder.Add("userNameTip", "请输入4399用户名");
        queryBuilder.Add("welcomeTip", "欢迎回到4399");
        queryBuilder.Add("sec", "1");
        return queryBuilder;
    }
}