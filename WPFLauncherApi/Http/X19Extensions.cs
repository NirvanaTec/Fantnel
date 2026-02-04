using System.Text.Json;
using WPFLauncherApi.Protocol;
using WPFLauncherApi.Utils;

namespace WPFLauncherApi.Http;

public class X19Extensions(string url, bool token = true) {
    public static readonly X19Extensions Gateway = new("https://x19apigatewayobt.nie.netease.com");
    public static readonly X19Extensions Client = new("https://x19mclobt.nie.netease.com");
    public static readonly X19Extensions Core = new("https://x19obtcore.nie.netease.com:8443", false);
    public static readonly X19Extensions Nirvana = new("http://110.42.70.32:13423", false);
    public static readonly X19Extensions Bmcl = new("https://bmclapi2.bangbang93.com", false);

    public readonly HttpWrapper HttpWrapper = new(url,
        options => { options.UserAgent("WPFLauncher/0.0.0.0"); });

    private async Task<HttpResponseMessage> Api(string url, string? body, string? userId, string? userToken)
    {
        if (body == null) return await HttpWrapper.GetAsync(url);

        return await HttpWrapper.PostAsync(url, body, "application/json",
            options => {
                if (userId != null && userToken != null)
                    options.AddHeaders(TokenUtil.Compute(url, body, userId, userToken));
                else if (token) options.AddHeaders(TokenUtil.Compute(url, body));
            });
    }

    public async Task<T?> Api<T>(string url)
    {
        return await Api<T>(url, null, null, null);
    }

    public async Task<T?> Api<T>(string url, object? body)
    {
        return await Api<T>(url, body, null, null);
    }

    public async Task<T?> Api<T>(string url, object? body, string? userId, string? userToken)
    {
        return await Api<T>(url, JsonSerializer.Serialize(body, WPFLauncher.DefaultOptions), userId, userToken);
    }

    public async Task<T?> Api<T>(string url, string body)
    {
        return await Api<T>(url, body, null, null);
    }

    private async Task<T?> Api<T>(string url, string? body, string? userId, string? userToken)
    {
        var response = await ApiRaw(url, body, userId, userToken);
        if (response == null) return default;
        if (typeof(T) == typeof(JsonDocument)) return (T)(object)JsonDocument.Parse(response);
        return JsonSerializer.Deserialize<T>(response);
    }

    private async Task<string?> ApiRaw(string url, string? body, string? userId, string? userToken)
    {
        var response = await Api(url, body, userId, userToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<TResult?> Api<TBody, TResult>(string url, TBody? body)
    {
        return await Api<TResult>(url, body);
    }
}