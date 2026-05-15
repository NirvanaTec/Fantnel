using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Nirvana.WPFLauncher.Entities.MPay;
using Nirvana.WPFLauncher.Http;
using Nirvana.WPFLauncher.Utils;
using Serilog;

namespace Nirvana.WPFLauncher.Protocol;

public class MPay : IDisposable {
    private static readonly JsonSerializerOptions DefaultOptions = new() {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly HttpWrapper _client = new();
    private readonly EntityDevice _device;

    private readonly HttpWrapper _service = new("https://service.mkey.163.com");

    private readonly string _unique;

    public MPay(string gameId, string gameVersion)
    {
        GameId = gameId;
        GameVersion = gameVersion;
        _unique = CreateOrLoadUnique(gameId).Result;
        _device = CreateOrLoadDeviceAsync(gameId).Result;
    }

    private string GameId { get; }

    private string GameVersion { get; }

    public void Dispose()
    {
        _client.Dispose();
        _service.Dispose();
        GC.SuppressFinalize(this);
    }

    public EntityDevice GetDevice()
    {
        return _device;
    }

    private static async Task<string> CreateOrLoadUnique(string gameId)
    {
        var text = gameId + "-guid.cds";
        var content = await File.ReadAllTextAsync(text);
        return string.IsNullOrEmpty(content) ? CreateUnique(text) : content;
    }

    private static string CreateUnique(string fileName)
    {
        var text = Guid.NewGuid().ToString().Replace("-", "");
        File.WriteAllText(fileName, text);
        return text;
    }

    private async Task<EntityDevice> CreateOrLoadDeviceAsync(string gameId)
    {
        var text = gameId + "-device.cds";
        var content = await File.ReadAllTextAsync(text);
        if (string.IsNullOrEmpty(content)) {
            return await CreateDeviceAsync(gameId, text);
        }

        return JsonSerializer.Deserialize<EntityDeviceResponse>(content)?.EntityDevice ?? throw new Exception("Failed to load device");
    }

    private async Task<EntityDevice> CreateDeviceAsync(string gameId, string fileName)
    {
        var buildDeviceParams = BuildDeviceParams();
        buildDeviceParams.Add("unique_id", _unique);
        var obj = await _service.PostAsync("/mpay/games/" + gameId + "/devices", buildDeviceParams.BuildQueryString(), "application/x-www-form-urlencoded");
        obj.EnsureSuccessStatusCode();
        var text = await obj.Content.ReadAsStringAsync();
        await File.WriteAllTextAsync(fileName, text);
        return JsonSerializer.Deserialize<EntityDeviceResponse>(text)?.EntityDevice ?? throw new Exception("Failed to create device");
    }

    public async Task<EntityMPayUserResponse> LoginWithEmailAsync(string email, string password)
    {
        var value = JsonSerializer.Serialize(new EntityUsersParameters {
            Password = password.EncodeMd5(),
            Unique = _unique,
            Username = email
        }, DefaultOptions).EncodeAes(_device.Key.DecodeHex()).EncodeHex();
        var queryString = BuildBaseParams();
        queryString.Add("opt_fields", "nickname,avatar,realname_status,mobile_bind_status,mask_related_mobile,related_login_status");
        queryString.Add("params", value);
        queryString.Add("un", email.EncodeBase64());
        var response = await _service.PostAsync($"/mpay/games/{GameId}/devices/{_device.Id}/users", queryString.BuildQueryString(), "application/x-www-form-urlencoded");
        var text = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) {
            var entityVerifyResponse = JsonSerializer.Deserialize<EntityVerifyResponse>(text);
            if (entityVerifyResponse is { Code: 1351 }) {
                throw new Exception(text);
            }

            throw new Exception("Failed to login with email, response: " + text);
        }

        return JsonSerializer.Deserialize<EntityMPayUserResponse>(text) ?? throw new Exception("Failed to login with email, response: " + text);
    }

    public async Task<bool> SendSmsCodeAsync(string phoneNumber)
    {
        var queryBuilder = BuildBaseParams();
        queryBuilder.Add("device_id", _device.Id);
        queryBuilder.Add("mobile", phoneNumber);
        var response = await _service.PostAsync("/mpay/api/users/login/mobile/get_sms", queryBuilder.BuildQueryString(), "application/x-www-form-urlencoded");
        var propertyValue = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) {
            Log.Error("Failed to send sms code, response: {Json}", propertyValue);
        }

        return response.IsSuccessStatusCode;
    }

    public async Task<EntitySmsTicket?> VerifySmsCodeAsync(string phoneNumber, string code)
    {
        var queryBuilder = BuildBaseParams();
        queryBuilder.Add("device_id", _device.Id);
        queryBuilder.Add("mobile", phoneNumber);
        queryBuilder.Add("smscode", code);
        queryBuilder.Add("up_content", "");
        var response = await _service.PostAsync("/mpay/api/users/login/mobile/verify_sms", queryBuilder.BuildQueryString(), "application/x-www-form-urlencoded");
        var text = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode) {
            return JsonSerializer.Deserialize<EntitySmsTicket>(text);
        }

        Log.Error("Failed to send sms code, response: {Json}", text);
        return null;
    }

    public async Task<EntityMPayUserResponse?> FinishSmsCodeAsync(string phoneNumber, string ticket)
    {
        var text = phoneNumber.EncodeBase64();
        var queryBuilder = BuildBaseParams();
        queryBuilder.Add("device_id", _device.Id);
        queryBuilder.Add("opt_fields", "nickname,avatar,realname_status,mobile_bind_status,mask_related_mobile,related_login_status");
        queryBuilder.Add("ticket", ticket);
        var response = await _service.PostAsync("/mpay/api/users/login/mobile/finish?un=" + text, queryBuilder.BuildQueryString(), "application/x-www-form-urlencoded");
        var text2 = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode) {
            return JsonSerializer.Deserialize<EntityMPayUserResponse>(text2);
        }

        Log.Error("Failed to finish sms code, response: {Json}", text2);
        return null;
    }


    private QueryBuilder BuildBaseParams()
    {
        var queryBuilder = new QueryBuilder();
        queryBuilder.Add("app_channel", "netease");
        queryBuilder.Add("app_mode", "2").Add("app_type", "games");
        queryBuilder.Add("arch", "win_x64");
        queryBuilder.Add("cv", "c4.2.0");
        queryBuilder.Add("mcount_app_key", "EEkEEXLymcNjM42yLY3Bn6AO15aGy4yq");
        queryBuilder.Add("mcount_transaction_id", "0");
        queryBuilder.Add("process_id", $"{Environment.ProcessId}");
        queryBuilder.Add("sv", "10.0.22621");
        queryBuilder.Add("updater_cv", "c1.0.0");
        queryBuilder.Add("game_id", GameId);
        queryBuilder.Add("gv", GameVersion);
        return queryBuilder;
    }

    private QueryBuilder BuildDeviceParams()
    {
        var queryBuilder = BuildBaseParams();
        queryBuilder.Add("brand", "Microsoft");
        queryBuilder.Add("device_model", "pc_mode");
        queryBuilder.Add("device_name", "PC-" + StringGenerator.GenerateRandomString(12));
        queryBuilder.Add("device_type", "Computer");
        queryBuilder.Add("init_urs_device", "0");
        queryBuilder.Add("mac", StringGenerator.GenerateRandomMacAddress());
        queryBuilder.Add("resolution", "1920x1080");
        queryBuilder.Add("system_name", "windows");
        queryBuilder.Add("system_version", "10.0.22621");
        return queryBuilder;
    }
}