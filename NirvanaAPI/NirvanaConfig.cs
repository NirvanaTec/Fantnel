using System.Text;
using System.Text.Json;
using NirvanaAPI.Entities.EntitiesNirvana;
using NirvanaAPI.Utils;
using NirvanaAPI.Utils.CodeTools;

namespace NirvanaAPI;

public class NirvanaConfig {

    public static EntityNirvanaConfig Config = new();

    private static readonly string FilePath = Path.Combine(PathUtil.ResourcePath, "nirvanaAccount.json");
    private static DateTime _lastApiCallTime = DateTime.MinValue;

    // 保存账号
    public static void SaveConfig()
    {
        lock (FilePath) {
            File.WriteAllText(FilePath, JsonSerializer.Serialize(Config), Encoding.UTF8);
        }
    }

    // 退出登录
    public static void Logout()
    {
        Config.Logout();
        SaveConfig();
    }

    // 登录检测
    public static bool IsLogin(bool useCache = true)
    {
        if (string.IsNullOrEmpty(Config.Account) || string.IsNullOrEmpty(Config.Token)) {
            throw new ErrorCodeException(ErrorCode.LogInNot);
        }

        return !useCache || IsUseCache();
    }

    // 是否使用缓存
    private static bool IsUseCache(int time = 5)
    {
        var now = DateTime.UtcNow;
        if ((now - _lastApiCallTime).TotalSeconds < time) {
            return true;
        }

        _lastApiCallTime = now;
        return false;
    }

    public static void SetHideAccount(string value)
    {
        Config.HideAccount = bool.Parse(value);
        SaveConfig();
    }

    public static void SetChatEnable(string value)
    {
        Config.ChatEnable = bool.Parse(value);
        SaveConfig();
    }

    public static void SetUseJavaW(string value)
    {
        Config.UseJavaW = bool.Parse(value);
        SaveConfig();
    }

    public static void SetAutoUpdatePlugin(string value)
    {
        Config.AutoUpdatePlugin = bool.Parse(value);
        SaveConfig();
    }

    public static void SetGameMemory(string value)
    {
        var gameMemory = int.Parse(value);
        if (gameMemory < 1024) {
            throw new ErrorCodeException(ErrorCode.MemoryError);
        }

        Config.GameMemory = gameMemory;
        SaveConfig();
    }

    public static void SetJvmArgs(string value)
    {
        Config.JvmArgs = value;
        SaveConfig();
    }

    public static void SetGameArgs(string value)
    {
        Config.GameArgs = value;
        SaveConfig();
    }

    public static void SetAutoLoginGame(string value)
    {
        Config.AutoLoginGame = bool.Parse(value);
        SaveConfig();
    }

    public static void SetAutoLoginGame163Email(string value)
    {
        Config.AutoLoginGame163Email = bool.Parse(value);
        SaveConfig();
    }

    public static void SetAutoLoginGameCookie(string value)
    {
        Config.AutoLoginGameCookie = bool.Parse(value);
        SaveConfig();
    }
    
}