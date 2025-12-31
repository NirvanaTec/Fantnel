using Microsoft.AspNetCore.Mvc;
using NirvanaPublic.Message;
using WPFLauncherApi.Utils.CodeTools;

namespace Fantnel.Servlet.PluginsController;

// plugin-store
[ApiController]
[Route("[controller]")]
public class PluginsShopController : ControllerBase
{
    [HttpGet("/api/pluginstore/get")]
    public IActionResult GetPluginListHttp()
    {
        var pluginList = PlugInstoreMessage.GetPluginList();
        return Content(Code.ToJson(ErrorCode.Success, pluginList), "application/json");
    }

    [HttpGet("/api/pluginstore/detail")]
    public IActionResult GetPluginDetailHttp([FromQuery] string id)
    {
        var pluginDetail = PlugInstoreMessage.GetPluginDetail(id);
        return Content(pluginDetail == null ? Code.ToJson(ErrorCode.NotFound) : Code.ToJson(pluginDetail),
            "application/json");
    }

    [HttpGet("/api/pluginstore/install")]
    public IActionResult DownloadHttp([FromQuery] string id)
    {
        PlugInstoreMessage.Install(id);
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }
}