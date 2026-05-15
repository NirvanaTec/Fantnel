using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Nirvana.WPFLauncher.Protocol;
using Nirvana.WPFLauncher.Utils;

namespace Nirvana.WPFLauncher.Http;

public class X19Extensions(string url, bool token = true) {
    public static readonly X19Extensions Gateway = new("https://x19apigatewayobt.nie.netease.com");
    public static readonly X19Extensions Client = new("https://x19mclobt.nie.netease.com");
    public static readonly X19Extensions Core = new("https://x19obtcore.nie.netease.com:8443", false);
    public static readonly X19Extensions Core1 = new("https://x19obtcore.nie.netease.com:8443");
    public static readonly X19Extensions Nirvana = new("http://110.42.70.32:13423", false);
    public static readonly X19Extensions Bmcl = new("https://bmclapi2.bangbang93.com", false);
    public static readonly X19Extensions Pt4399 = new("https://ptlogin.4399.com", false);
    public static readonly X19Extensions UpdateNetease = new("https://x19.update.netease.com", false);
    public readonly HttpWrapper HttpWrapper = new(url, options => {
        options.UserAgent("WPFLauncher/0.0.0.0");
    });

    private async Task<HttpResponseMessage> ApiSend(string url, string? body = null, string? userId = null, string? userToken = null)
    {
        if (body == null) {
            return await HttpWrapper.GetAsync(url);
        }

        return await HttpWrapper.PostAsync(url, body, "application/json", options => {
            if (userId != null && userToken != null) {
                options.AddHeaders(TokenUtil.Compute(url, body, userId, userToken));
            } else if (token) {
                options.AddHeaders(TokenUtil.Compute(url, body));
            }
        });
    }

    private async Task<HttpResponseMessage> ApiSendBytes(string url, byte[]? body = null)
    {
        return body == null ? await HttpWrapper.GetAsync(url) : await HttpWrapper.PostAsync(url, body);
    }

    private async Task<string> Api(string url, byte[]? body = null)
    {
        var response = await ApiSendBytes(url, body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<T?> ApiBytes<T>(string url, byte[]? body = null)
    {
        var response = await Api(url, body);
        return ToType<T>(response);
    }

    public async Task<T?> Api<T>(string url, object? body = null, string? userId = null, string? userToken = null)
    {
        return await Api<T>(url, body == null ? null : JsonSerializer.Serialize(body, NPFLauncher.DefaultOptions), userId, userToken);
    }

    private async Task<T?> Api<T>(string url, string? body = null, string? userId = null, string? userToken = null)
    {
        var response = await ApiRawByString(url, body, userId, userToken);
        return ToType<T>(response);
    }

    private static T? ToType<T>(string? response)
    {
        if (response == null) {
            return default;
        }

        if (typeof(T) == typeof(JsonDocument)) {
            return (T)(object)JsonDocument.Parse(response);
        }

        if (typeof(T) == typeof(string)) {
            return (T)(object)response;
        }

        return JsonSerializer.Deserialize<T>(response);
    }

    private async Task<string?> ApiRawByString(string url, string? body = null, string? userId = null, string? userToken = null)
    {
        var response = await ApiSend(url, body, userId, userToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<byte[]?> ApiRawB(string url, string? body = null, string? userId = null, string? userToken = null)
    {
        var response = await ApiSend(url, body, userId, userToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }
}