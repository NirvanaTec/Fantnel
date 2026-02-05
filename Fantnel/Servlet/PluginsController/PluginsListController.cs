using Microsoft.AspNetCore.Mvc;
using NirvanaAPI.Utils.CodeTools;
using NirvanaPublic.Message;

namespace Fantnel.Servlet.PluginsController;

// plugins
[ApiController]
[Route("[controller]")]
public class PluginsListController : ControllerBase {
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

    [HttpGet("/api/plugins/dependence")]
    public IActionResult GetDependenceListHttp(string? id = null, string? version = null)
    {
        var entity = PluginMessage.GetDependenceList(id, version).Result;
        var support = false; // 无支持插件
        var size = 0; // 插件数量
        foreach (var entity1 in entity.ToList()) {
            foreach (var entity2 in entity1.Data) {
                if (PluginMessage.IsPluginExist(entity2.Id)) {
                    support = true; // 有支持插件
                    entity.Remove(entity1);
                }

                size++;
            }
        }

        // 无支持插件 并 没有可安装的插件
        if (!support && size == 0) {
            return Content(Code.ToJson(ErrorCode.GamePlugin), "application/json");
        }

        return Content(Code.ToJson(ErrorCode.Success, entity), "application/json");
    }
}