using System.Text.Json;
using Codexus.OpenSDK;
using NirvanaPublic.Manager;

namespace NirvanaPublic.Utils;

public static class X19Extensions
{
    private static async Task<HttpResponseMessage> Api(string url, string body)
    {
        return await X19.ApiPostAsync(url, body, InfoManager.GetGameAccount().GetUserId(),
            InfoManager.GetGameAccount().GetToken());
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
        var response = await ApiRaw(url, JsonSerializer.Serialize(body));
        if (response == null) return default;
        if (typeof(TResult) == typeof(JsonDocument)) return (TResult)(object)JsonDocument.Parse(response);
        return JsonSerializer.Deserialize<TResult>(response);
    }
}