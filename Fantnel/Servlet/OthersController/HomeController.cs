using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using NirvanaAPI.Entities.EntitiesNirvana;
using NirvanaAPI.Manager;
using NirvanaAPI.Utils;
using NirvanaAPI.Utils.CodeTools;
using NirvanaPublic;
using NirvanaPublic.Utils;
using NirvanaPublic.Utils.Update;

namespace Fantnel.Servlet.OthersController;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase {
    // 设置主题
    [HttpGet("/api/theme/set")]
    public IActionResult SetTheme(string name)
    {
        ConfigUtil.SaveConfig("theme", name);
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }

    // 获取主题
    [HttpGet("/api/theme")]
    public IActionResult GetTheme()
    {
        // 从配置中获取主题
        var theme = ConfigUtil.GetConfig()["theme"] ?? "default";
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
        var version = new JsonObject {
            ["version"] = PublicProgram.Version,
            ["id"] = PublicProgram.VersionId,
            ["mode"] = PublicProgram.Mode,
            ["arch"] = PublicProgram.Arch
        };
        return Content(Code.ToJson(ErrorCode.Success, version), "application/json");
    }

    // 设置主题
    [HttpGet("/api/reboot")]
    public IActionResult Reboot()
    {
        // 重启程序
        Tools.Restart();
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }

    // 设置主题
    [HttpPost("/api/theme/switch")]
    public IActionResult ThemeSwitch([FromBody] EntityValue entity)
    {
        if (string.IsNullOrEmpty(entity.Value)) {
            return Content(Code.ToJson(ErrorCode.ParamError), "application/json");
        }

        if (InitProgram.SafeTheme(entity.Value).Result) {
            ConfigUtil.SaveConfig("themeValue", entity.Value);
        }

        UpdateTools.CheckUpdate("ui." + entity.Value).Wait();
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }

    public static string GetIndexHtml()
    {
        // 获取运行目录路径
        var resourcesPath = Path.Combine(PathUtil.ResourcePath, "static", "index.html");
        return System.IO.File.Exists(resourcesPath) ? System.IO.File.ReadAllText(resourcesPath) : "";
    }
}