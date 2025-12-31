private static async Task<Dictionary<string, string>> GetUniAuthAsync(Dictionary<string, string> queryParams, HttpClient client)
{
    var sdkUrl = "https://microgame.5054399.net/v2/service/sdk/info?" +
                 "callback=&queryStr=game_id%3D500352%26nick%3Dnull%26sig%3D" + queryParams["sig"] + "%26" +
                 "uid%3D" + queryParams["uid"] + "%26fcm%3D0%26show%3D1%26isCrossDomain%3D1%26rand_time%3D" +
                 queryParams["rand_time"] + "%26" +
                 "ptusertype%3D4399%26time%3D" + queryParams["time"] + "%26validateState%3D" +
                 queryParams["validateState"] + "%26" +
                 "username%3D" + queryParams["username"] + "&_=" + queryParams["time"];

    var response = await client.GetAsync(sdkUrl);

    if (!response.IsSuccessStatusCode)
        throw new Exception("获取统一认证信息失败");

    var responseText = await response.Content.ReadAsStringAsync();
    var uniAuthData = JsonSerializer.Deserialize<C4399UniAuth>(responseText) ?? throw new Exception("解析统一认证数据失败");

    // 将 SdkLoginData 转换为字典
    var result = new Dictionary<string, string>();
    if (uniAuthData.Data?.SdkLoginData != null)
    {
        foreach (var prop in uniAuthData.Data.SdkLoginData.GetType().GetProperties())
        {
            var value = prop.GetValue(uniAuthData.Data.SdkLoginData)?.ToString();
            if (value != null)
            {
                result[prop.Name] = value;
            }
        }
    }
    return result;
}

private static Dictionary<string, string> ExtractQueryParameters(string url)
{
    var result = new Dictionary<string, string>();
    var queryStart = url.IndexOf('?');
    if (queryStart == -1) return result;

    var queryString = url[(queryStart + 1)..];
    var parameters = queryString.Split('&');

    foreach (var param in parameters)
    {
        var parts = param.Split('=');
        if (parts.Length == 2) 
        {
            result[parts[0]] = Uri.UnescapeDataString(parts[1]);
        }
    }
    return result;
}