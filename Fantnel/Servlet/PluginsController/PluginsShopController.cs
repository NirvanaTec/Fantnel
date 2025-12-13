using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NirvanaPublic.Message;
using NirvanaPublic.Utils.ViewLogger;

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
        return Content(Code.ToJson(Code.ErrorCode.Success, pluginList), "application/json");
    }

    [HttpGet("/api/pluginstore/detail")]
    public IActionResult GetPluginDetailHttp([FromQuery] string id)
    {
        var pluginDetail = PlugInstoreMessage.GetPluginDetail(id);
        return Content(pluginDetail == null ? Code.ToJson(Code.ErrorCode.NotFound) : Code.ToJson(pluginDetail),
            "application/json");
    }

    [HttpGet("/api/pluginstore/install")]
    public IActionResult DownloadHttp([FromQuery] string id)
    {
        lock (id)
        {
            var downloadInfo = PlugInstoreMessage.GetDownloadInfoUrl(id);
            if (downloadInfo?.Data == null || downloadInfo.Code != 1)
            {
                return Content(JsonSerializer.Serialize(downloadInfo), "application/json");
            }

            // 检测 插件
            if (PlugInstoreMessage.NoEqualsPlugin(downloadInfo.Data.FileHash, downloadInfo.Data.FileSize))
            {
                PlugInstoreMessage.Download(id);
            }

            // 依赖插件 为空 则 直接返回成功
            if (downloadInfo.Data?.Dependencies == null)
            {
                return Content(Code.ToJson(Code.ErrorCode.Success), "application/json");
            }
            
            // 检测 依赖插件
            foreach (var item in downloadInfo.Data.Dependencies)
            {
                if (PlugInstoreMessage.NoEqualsPlugin(item.FileHash, item.FileSize))
                {
                    PlugInstoreMessage.Download(item.Id);
                }
            }

            return Content(Code.ToJson(Code.ErrorCode.Success), "application/json");
        }
    }

}