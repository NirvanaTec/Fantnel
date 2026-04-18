using System.Net;
using System.Text.Json;
using Nirvana.WPFLauncher.Entities.EntitiesPc4399.Com4399;
using Nirvana.WPFLauncher.Utils;

namespace Nirvana.WPFLauncher.Protocol;

public static class NCom4399 {
    
    private static readonly HttpClient Client = new(new HttpClientHandler {
        UseCookies = true,
        CookieContainer = new CookieContainer()
    });

    public static async Task<string> LoginWithPasswordAsync(
        string username,
        string password,
        string sessionId,
        string captcha)
    {
        var oauthResp = await Client.GetAsync("https://m.4399api.com/openapi/oauth-callback.html?gamekey=44770&game_key=115716");
        var oauthText = await oauthResp.Content.ReadAsStringAsync();
        var oauthCallback = JsonSerializer.Deserialize<Entity4399OAuthCallback>(oauthText);
        if (oauthCallback == null) {
            throw new Exception("Failed to deserialize: " + oauthText);
        }

        var queryParams = QueryBuilder.FromParameters(oauthCallback.Result);
        var clientId = queryParams.Get("client_id");
        var state = queryParams.Get("state");
        var ref1 = queryParams.Get("ref");
        
        // 构建登录参数
        var parameters = BuildLoginParameters(clientId, state, ref1);
        parameters.Add("username", username);
        parameters.Add("password", password);
        parameters.Add("captcha_id", captcha);
        parameters.Add("captcha", sessionId);
        
        // 执行登录请求
        var loginResponse = await Client.PostAsync("https://ptlogin.4399.com/oauth2/loginAndAuthorize.do?channel=&sdk=op&sdk_version=3.14.5.577",
            new FormUrlEncodedContent(parameters.GetAll()));

        var loginText = await loginResponse.Content.ReadAsStringAsync();

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
        return MgbSdk.GenerateSAuth(
            entity4399UserInfoResult.Uid.ToString(),
            entity4399UserInfoResult.State,
            "4399com",
            "ad"
        );
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

    private static QueryBuilder BuildLoginParameters(string clientId, string state, string ref1)
    {
        var queryBuilder = new QueryBuilder();
        queryBuilder.Add("isInputRealname", "false");
        queryBuilder.Add("isVaildRealname", "false");
        queryBuilder.Add("sec", "0");
        queryBuilder.Add("client_id", clientId);
        queryBuilder.Add("state", state);
        queryBuilder.Add("ref", ref1);
        queryBuilder.Add("response_type", "TOKEN");
        queryBuilder.Add("scope", "basic");
        queryBuilder.Add("bizId", "2100001792");
        queryBuilder.Add("auth_action", "ORILOGIN");
        queryBuilder.Add("redirect_uri", "https://m.4399api.com/openapi/oauth-callback.html?gamekey=44770&game_key=115716");
        return queryBuilder;
    }

}