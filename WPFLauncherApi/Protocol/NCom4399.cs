using System.Net;
using System.Text.Json;
using WPFLauncherApi.Entities.EntitiesPc4399;
using WPFLauncherApi.Entities.EntitiesPc4399.Com4399;
using QueryBuilder = WPFLauncherApi.Utils.QueryBuilder;

namespace WPFLauncherApi.Protocol;

public static class NCom4399 {
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
        parameters.Add("captcha_id", captcha);
        parameters.Add("captcha", sessionId);

        var client = new HttpClient(new HttpClientHandler {
            UseCookies = true,
            CookieContainer = new CookieContainer()
        });

        // 执行登录请求
        var loginResponse = client.PostAsync("https://ptlogin.4399.com/oauth2/loginAndAuthorize.do?channel=",
            new FormUrlEncodedContent(parameters.GetAll())).Result;

        var aaa = parameters.BuildQueryString();

        if (!loginResponse.IsSuccessStatusCode) {
            throw new Exception("登录请求失败");
        }

        var loginText = loginResponse.Content.ReadAsStringAsync().Result;

        // 找到错误信息
        var errText = ExtractErrorTip(loginText);
        if (errText.Length > 0) {
            throw new Exception(errText);
        }

        var userInfoResponse = JsonSerializer.Deserialize<Entity4399UserInfoResponse>(loginText);

        if (userInfoResponse == null) {
            throw new Exception("Failed to deserialize: " + loginText);
        }

        if (userInfoResponse.Code != "100") {
            throw new Exception(userInfoResponse.Message);
        }

        var entity4399UserInfoResult = userInfoResponse.Result;
        if (entity4399UserInfoResult == null) {
            throw new Exception("Failed to deserialize: " + loginText);
        }

        // 生成SAuth令牌
        return GenerateSAuth(
            "",
            entity4399UserInfoResult.Uid.ToString(),
            entity4399UserInfoResult.State,
            "",
            "4399com"
        );
    }

    private static async Task<string> OAuthDevice()
    {
        var client = new HttpClient(new HttpClientHandler {
            UseCookies = true,
            CookieContainer = new CookieContainer()
        });

        var parameters = BuildAuthDevice();

        // 执行登录请求
        var loginResponse = await client.PostAsync("https://m.4399api.com/openapiv2/oauth.html",
            new FormUrlEncodedContent(parameters.GetAll()));

        var loginText = await loginResponse.Content.ReadAsStringAsync();
        var oauthResponse = JsonSerializer.Deserialize<Entity4399OAuthResponse>(loginText);
        if (oauthResponse == null) {
            throw new Exception("Failed to deserialize: " + loginText);
        }

        var queryParams = QueryBuilder.FromParameters(oauthResponse.Result.LoginUrl);

        return queryParams.Get("state");
    }

    private static string ExtractErrorTip(string html)
    {
        const string startMarker = "login_err_msg\">";
        const string endMarker = "</p>";

        var startIndex = html.IndexOf(startMarker, StringComparison.Ordinal);
        if (startIndex == -1) return string.Empty;

        startIndex += startMarker.Length;
        var endIndex = html.IndexOf(endMarker, startIndex, StringComparison.Ordinal);

        if (endIndex == -1) return string.Empty;

        // 提取内容并删除前后空格
        var content = html.Substring(startIndex, endIndex - startIndex);
        return content.Trim();
    }

    private static string GenerateSAuth(
        string userId,
        string sdkUid,
        string sessionId,
        string timestamp,
        string channel,
        string platform = "ad")
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

    private static QueryBuilder BuildLoginParameters()
    {
        // _d
        var queryBuilder = new QueryBuilder();
        queryBuilder.Add("_d", "");
        queryBuilder.Add("access_token", "");
        queryBuilder.Add("aid", "");
        queryBuilder.Add("auth_action", "ORILOGIN");
        queryBuilder.Add("auto_scroll", "");
        queryBuilder.Add("autoCreateAccount", "");
        queryBuilder.Add("bizId", "2100001792");
        queryBuilder.Add("cid", "");
        queryBuilder.Add("client_id", "40f9e9b95d6c71ba5c6e0bd14c0abeff");
        queryBuilder.Add("css", "");
        queryBuilder.Add("expand_ext_login_list", "");
        queryBuilder.Add("game_key", "115716");
        queryBuilder.Add("isInputRealname", "false");
        queryBuilder.Add("isValidRealname", "false");
        queryBuilder.Add("redirect_uri", "https://m.4399api.com/openapi/oauth-callback.html?gamekey=44770");
        queryBuilder.Add("ref", "{\"game\":\"115716\",\"channel\":\"\"}");
        queryBuilder.Add("reg_mode", "reg_phone");
        queryBuilder.Add("response_type", "TOKEN");
        queryBuilder.Add("scope", "basic");
        queryBuilder.Add("sec", "1");
        queryBuilder.Add("show_4399", "");
        queryBuilder.Add("show_back_button", "");
        queryBuilder.Add("show_close_button", "");
        queryBuilder.Add("show_ext_login", "");
        queryBuilder.Add("show_forget_password", "");
        queryBuilder.Add("show_topbar", "false");
        queryBuilder.Add("state", OAuthDevice().Result);
        queryBuilder.Add("uid", "");
        queryBuilder.Add("username_history", "");
        return queryBuilder;
    }

    private static QueryBuilder BuildAuthDevice()
    {
        var queryBuilder = new QueryBuilder();
        queryBuilder.Add("usernames", "");
        queryBuilder.Add("top_bar", "1");
        queryBuilder.Add("state", "");
        queryBuilder.Add("device", JsonSerializer.Serialize(new Entity4399OAuth {
            DeviceIdentifier = string.Empty,
            DeviceIdentifierSm = string.Empty,
            Udid = string.Empty
        }));
        return queryBuilder;
    }
}