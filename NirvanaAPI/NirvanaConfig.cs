using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using NirvanaAPI.Entities;
using NirvanaAPI.Utils;
using NirvanaAPI.Utils.CodeTools;

namespace NirvanaAPI;

public class NirvanaConfig {
    
    private static readonly string FilePath = Path.Combine(PathUtil.ResourcePath, "nirvanaAccount.json");

    private static List<ConfigValue> ConfigValues { get; } = [
        new() {
            Name = "hideAccount", Default = "true", PropertyName = "bool"
        }, // 隐藏账号
        new() {
            Name = "chatEnable", Default = "true", PropertyName = "bool"
        }, // 聊天功能
        new() {
            Name = "gameMemory", Default = "4096", PropertyName = "int"
        }, // 游戏内存
        new() {
            Name = "jvmArgs", Default = "-XX:+UseG1GC -XX:-UseAdaptiveSizePolicy -XX:-OmitStackTraceInFastThrow -Djdk.lang.Process.allowAmbiguousCommands=true -Dfml.ignoreInvalidMinecraftCertificates=True -Dfml.ignorePatchDiscrepancies=True -Dlog4j2.formatMsgNoLookups=true"
        }, // 虚拟机参数
        new() {
            Name = "gameArgs", Default = ""
        }, // 游戏参数
        new() {
            Name = "autoLoginGame", Default = "true", PropertyName = "bool"
        }, // 主动登录游戏
        new() {
            Name = "autoLoginGame163Email", Default = "false", PropertyName = "bool"
        }, // 主动登录 163Email
        new() {
            Name = "autoLoginGameCookie", Default = "true", PropertyName = "bool"
        }, // 主动登录 Cookie
        new() {
            Name = "useJavaW", Default = "true", PropertyName = "bool"
        }, // 使用 javaw.exe
        new() {
            Name = "autoUpdatePlugin", Default = "true", PropertyName = "bool"
        }, // 自动更新插件
        new() {
            Name = "account", Default = ""
        }, // 涅槃账号
        new() {
            Name = "token", Default = ""
        } // 涅槃在线密钥
    ];

    // 初始化
    public static void Initialization()
    {
        lock (FilePath) {
            var entity = Tools.GetValueOrDefault<JsonObject>("nirvanaAccount.json").Item1;
            if (entity == null) {
                return;
            }
            foreach (var configValue in entity) {
                try {
                    SetValue(configValue.Key, configValue.Value?.ToString());
                } catch (Exception) {
                    AddValue(configValue.Key, configValue.Value);
                }
            }
        }
    }

    public static bool GetBool(string name)
    {
        return bool.Parse(GetString(name));
    }

    public static string GetString(string name)
    {
        var configValue = GetValueTo(name);
        return configValue == null ? throw new Exception($"Config {name} not found") : configValue.GetValue();
    }

    private static ConfigValue? GetValueTo(string name, string? propertyName = null)
    {
        return ConfigValues.Where(configValue => configValue.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault(configValue => string.IsNullOrEmpty(propertyName) || propertyName.Equals(configValue.PropertyName, StringComparison.OrdinalIgnoreCase));
    }

    public static void SetValue(string name, string? value, string? propertyName = null)
    {
        var configValue = GetValueTo(name, propertyName);
        if (configValue == null) {
            throw new Exception($"Config {name} not found");
        }
        configValue.SetValue(value);
        SaveConfig();
    }

    private static void AddValue(string name, JsonNode? jsonNode)
    {
        var configValue = new ConfigValue {
            Name = name,
            Default = string.Empty,
            PropertyName = string.Empty
        };
        if (jsonNode != null) {
            configValue.SetValue(jsonNode.ToString());
            configValue.PropertyName = jsonNode.GetValueKind() switch {
                JsonValueKind.Number => "number",
                JsonValueKind.String => "string",
                JsonValueKind.False or JsonValueKind.True => "bool",
                _ => configValue.PropertyName
            };
        }
        ConfigValues.Add(configValue);
    }
    
    public static void AddValue(string name, string? defaultValue, string? propertyName)
    {
        var findValue = GetValueTo(name);
        if (findValue != null) {
            if (defaultValue != null) {
                findValue.Default = defaultValue;
            }
            if (propertyName != null) {
                findValue.PropertyName = propertyName;
            }
        } else {
            var configValue = new ConfigValue {
                Name = name,
                Default = defaultValue ?? string.Empty,
                PropertyName = propertyName ?? string.Empty
            };
            ConfigValues.Add(configValue);
        }
    }

    private static string GetString(bool showDefault = true)
    {
        return JsonSerializer.Serialize(GetJsonObject(showDefault));
    }

    public static JsonObject GetJsonObject(bool showDefault = true)
    {
        var jsonObj = new JsonObject();
        foreach (var configValue in ConfigValues) {
            if (!showDefault && configValue.IsDefault()) {
                continue;
            }
            var value = configValue.GetValue();
            if (configValue.IsProperty("bool")) {
                jsonObj.Add(configValue.Name, bool.Parse(value));
            } else if (configValue.IsProperty("number") || configValue.IsProperty("double")) {
                jsonObj.Add(configValue.Name, double.Parse(value));
            } else if (configValue.IsProperty("float")) {
                jsonObj.Add(configValue.Name, float.Parse(value));
            } else if (configValue.IsProperty("int")) {
                jsonObj.Add(configValue.Name, int.Parse(value));
            } else {
                jsonObj.Add(configValue.Name, value);
            }
        }
        return jsonObj;
    }

    // 保存账号
    private static void SaveConfig()
    {
        lock (FilePath) {
            File.WriteAllText(FilePath, GetString(false), Encoding.UTF8);
        }
    }

    // 退出登录
    public static void Logout()
    {
        SetValue("account", "");
        SetValue("token", "");
        SaveConfig();
    }

    // 登录检测
    public static void IsLogin()
    {
        var account = GetString("account");
        var token = GetString("token");
        if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(token)) {
            throw new ErrorCodeException(ErrorCode.LogInNot);
        }
    }

    public static void SetGameMemory(string? value)
    {
        if (string.IsNullOrEmpty(value)) {
            throw new ErrorCodeException(ErrorCode.MemoryError);
        }

        var gameMemory = int.Parse(value);
        if (gameMemory < 1024) {
            throw new ErrorCodeException(ErrorCode.MemoryError);
        }

        SetValue("gameMemory", gameMemory.ToString());
    }

    public static string GetLoginT()
    {
        var account = GetString("account");
        var token = GetString("token");
        return $"account={account}&online={token}";
    }
}