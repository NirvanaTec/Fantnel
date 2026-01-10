using Microsoft.AspNetCore.Mvc;
using NirvanaPublic.Message;
using WPFLauncherApi.Utils.CodeTools;

namespace Fantnel.Servlet.PluginsController;

// plugins
[ApiController]
[Route("[controller]")]
public class PluginsListController : ControllerBase
{
    [HttpGet("/api/plugins/get")]
    public IActionResult GetPluginsListHttp()
    {
        var entity = PluginMessage.GetPluginListSafe();
        return Content(Code.ToJson(ErrorCode.Success, entity), "application/json");
    }

    [HttpGet("/api/plugins/toggle")]
    public IActionResult TogglePluginHttp(string id)
    {
        PluginMessage.TogglePlugin(id);
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }

    [HttpGet("/api/plugins/delete")]
    public IActionResult DeletePluginHttp(string id)
    {
        PluginMessage.DeletePlugin(id);
        // 避免执行过快
        Thread.Sleep(1000);
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }
}