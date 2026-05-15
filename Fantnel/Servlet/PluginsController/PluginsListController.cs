using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Nirvana.Development.Manager;
using Nirvana.Public.Message;
using NirvanaAPI.Utils.CodeTools;

namespace Fantnel.Servlet.PluginsController;

// plugins
[ApiController]
[Route("[controller]")]
public class PluginsListController : ControllerBase {
    [HttpGet("/api/plugins/get")]
    public IActionResult GetPluginsListHttp()
    {
        var entity = PluginManager.GetPluginStates();
        return Ok(Code.ToJson(ErrorCode.Success, entity));
    }

    [HttpGet("/api/plugins/toggle")]
    public IActionResult TogglePluginHttp(string id)
    {
        PluginManager.TogglePlugin(id);
        return Ok(Code.ToJson(ErrorCode.Success));
    }

    [HttpGet("/api/plugins/delete")]
    public IActionResult DeletePluginHttp(string id)
    {
        PluginManager.DeletePlugin(id);
        // 避免执行过快
        Thread.Sleep(1000);
        return Ok(Code.ToJson(ErrorCode.Success));
    }

    [HttpGet("/api/plugins/dependence")]
    public IActionResult GetDependenceListHttp(string? id = null, string? version = null)
    {
        var entity = PluginMessage.GetDependenceList(id, version);
        return Ok(Code.ToJson(ErrorCode.Success, entity));
    }
}