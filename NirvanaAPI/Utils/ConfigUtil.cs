using System.Text.Json;
using System.Text.Json.Nodes;

namespace NirvanaAPI.Utils;

public static class ConfigUtil {
    // 获取配置
    public static JsonObject GetConfig()
    {
        var resourcesPath = Path.Combine(PathUtil.ResourcePath, "config.json");
        if (!File.Exists(resourcesPath)) return new JsonObject();
        return JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(resourcesPath)) ?? new JsonObject();
    }

    // 保存配置
    private static void SaveConfig(JsonObject config)
    {
        var resourcesPath = Path.Combine(PathUtil.ResourcePath, "config.json");
        File.WriteAllText(resourcesPath, JsonSerializer.Serialize(config));
    }

    // 保存配置
    public static void SaveConfig(string name, string value)
    {
        var config = GetConfig();
        config[name] = value;
        SaveConfig(config);
    }
}