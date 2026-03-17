using Microsoft.AspNetCore.Mvc;
using Nirvana.Public.Message;
using NirvanaAPI.Utils.CodeTools;

namespace Fantnel.Servlet.PluginsController;

// plugin-store
[ApiController]
[Route("[controller]")]
public class PluginsShopController : ControllerBase {
    [HttpGet("/api/pluginstore/get")]
    public IActionResult GetPluginListHttp()
    {
        var pluginList = PlugInstoreMessage.GetPluginList().Result;
        return Ok(Code.ToJson(ErrorCode.Success, pluginList));
    }

    [HttpGet("/api/pluginstore/detail")]
    public IActionResult GetPluginDetailHttp([FromQuery] string id)
    {
        var pluginDetail = PlugInstoreMessage.GetPluginDetail(id);
        return Ok(pluginDetail == null ? Code.ToJson(ErrorCode.NotFound) : pluginDetail);
    }

    [HttpGet("/api/pluginstore/install")]
    public IActionResult DownloadHttp([FromQuery] string id)
    {
        PlugInstoreMessage.Install(id);
        return Ok(Code.ToJson(ErrorCode.Success));
    }
}