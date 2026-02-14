using System.Text;
using System.Text.Json;
using NirvanaAPI.Entities.EntitiesNirvana;
using NirvanaAPI.Utils;
using NirvanaAPI.Utils.CodeTools;

namespace NirvanaAPI;

public class NirvanaConfig {
    
    public const string JvmArgsConst =
        "-XX:-UseAdaptiveSizePolicy -XX:-OmitStackTraceInFastThrow -Djdk.lang.Process.allowAmbiguousCommands=true -Dfml.ignoreInvalidMinecraftCertificates=True -Dfml.ignorePatchDiscrepancies=True -Dlog4j2.formatMsgNoLookups=true"; // 虚拟机参数

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
        Config.SetHideAccount(value);
        SaveConfig();
    }

    public static void SetChatEnable(string value)
    {
        Config.SetChatEnable(value);
        SaveConfig();
    }

    public static void SetChatTarget(string value)
    {
        Config.SetChatTarget(value);
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

    public static void SetJvmArgs(string? value)
    {
        Config.JvmArgs = value ?? JvmArgsConst;
        SaveConfig();
    }

    public static void SetGameArgs(string value)
    {
        Config.GameArgs = value;
        SaveConfig();
    }

    public static void SetChatPrefix(string value)
    {
        Config.ChatPrefix = value;
        SaveConfig();
    }

    public static void SetAutoLoginGame(string value)
    {
        Config.AutoLoginGame = "true".Equals(value);
        SaveConfig();
    }

    public static void SetAutoLoginGame4399(string value)
    {
        Config.AutoLoginGame4399 = "true".Equals(value);
        SaveConfig();
    }

    public static void SetAutoLoginGame4399Com(string value)
    {
        Config.AutoLoginGame4399Com = "true".Equals(value);
        SaveConfig();
    }

    public static void SetAutoLoginGame163Email(string value)
    {
        Config.AutoLoginGame163Email = "true".Equals(value);
        SaveConfig();
    }

    public static void SetAutoLoginGameCookie(string value)
    {
        Config.AutoLoginGameCookie = "true".Equals(value);
        SaveConfig();
    }
    
}