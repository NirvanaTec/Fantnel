using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using NirvanaPublic;
using NirvanaPublic.Manager;
using WPFLauncherApi.Utils.CodeTools;

namespace Fantnel.Servlet;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    // 设置主题
    [HttpGet("/api/theme/set")]
    public IActionResult SetTheme(string name)
    {
        SaveConfig("theme", name);
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }

    // 获取主题
    [HttpGet("/api/theme")]
    public IActionResult GetTheme()
    {
        // 从配置中获取主题
        var theme = GetConfig()["theme"] ?? "default";
        return Content(Code.ToJson(ErrorCode.Success, theme), "application/json");
    }

    // 获取首页信息
    [HttpGet("/api/home")]
    public IActionResult HomeInfo()
    {
        return Content(JsonSerializer.Serialize(InfoManager.FantnelInfo), "application/json");
    }

    // 获取版本
    [HttpGet("/api/version")]
    public IActionResult GetVersion()
    {
        var version = new JsonObject
        {
            ["version"] = PublicProgram.Version,
            ["id"] = PublicProgram.VersionId,
            ["mode"] = PublicProgram.Mode
        };
        return Content(Code.ToJson(ErrorCode.Success, version), "application/json");
    }

    public static string GetIndexHtml()
    {
        // 获取运行目录路径
        var resourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "resources", "static", "index.html");
        return System.IO.File.Exists(resourcesPath) ? System.IO.File.ReadAllText(resourcesPath) : "";
    }

    // 获取配置
    private static JsonObject GetConfig()
    {
        var resourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "resources", "config.json");
        if (!System.IO.File.Exists(resourcesPath)) return new JsonObject();
        return JsonSerializer.Deserialize<JsonObject>(System.IO.File.ReadAllText(resourcesPath)) ?? new JsonObject();
    }

    // 保存配置
    private static void SaveConfig(JsonObject config)
    {
        var resourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "resources", "config.json");
        System.IO.File.WriteAllText(resourcesPath, JsonSerializer.Serialize(config));
    }

    // 保存配置
    private static void SaveConfig(string name, string value)
    {
        var config = GetConfig();
        config[name] = value;
        SaveConfig(config);
    }
}