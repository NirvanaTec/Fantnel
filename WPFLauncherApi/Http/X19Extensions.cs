using System.Text.Json;
using WPFLauncherApi.Protocol;
using WPFLauncherApi.Utils.CodeTools;

namespace WPFLauncherApi.Http;

public static class X19Extensions
{
    private static async Task<HttpResponseMessage> Api(string url, string body)
    {
        if (PublicProgram.User.UserId == null || PublicProgram.User.Token == null)
            throw new ErrorCodeException(ErrorCode.LogInNot);
        return await X19.PostAsync(url, body);
    }

    public static async Task<T?> Api<T>(string url, object? body)
    {
        return await Api<T>(url, JsonSerializer.Serialize(body, WPFLauncher.DefaultOptions));
    }

    public static async Task<T?> Api<T>(string url, string body)
    {
        var response = await ApiRaw(url, body);
        if (response == null) return default;
        if (typeof(T) == typeof(JsonDocument)) return (T)(object)JsonDocument.Parse(response);
        return JsonSerializer.Deserialize<T>(response);
    }

    private static async Task<string?> ApiRaw(string url, string body)
    {
        var response = await Api(url, body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<TResult?> Api<TBody, TResult>(string url, TBody body)
    {
        return await Api<TResult>(url, body);
    }
}