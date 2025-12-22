using System.Text.Json;
using Codexus.OpenSDK;
using NirvanaPublic.Manager;

namespace NirvanaPublic.Utils;

public static class X19Extensions
{
    private static async Task<HttpResponseMessage> Api(string url, string body)
    {
        return await X19.ApiPostAsync(url, body, InfoManager.GetGameUser().UserId,
            InfoManager.GetGameUser().AccessToken);
    }

    public static async Task<T?> Api<T>(string url, string body)
    {
        var response = await Api(url, body);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json);
    }

    public static async Task<TResult?> Api<TBody, TResult>(string url, TBody body)
    {
        var response = await Api(url, JsonSerializer.Serialize(body));
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResult>(json);
    }
}