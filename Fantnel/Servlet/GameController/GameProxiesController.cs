using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using NirvanaPublic.Message;
using NirvanaPublic.Utils;
using WPFLauncherApi.Utils.CodeTools;

namespace Fantnel.Servlet.GameController;

[ApiController]
[Route("[controller]")]
public class GameProxiesController : ControllerBase
{
    [HttpGet("/api/gameserver/launch")]
    public IActionResult LaunchGame([FromQuery] string id, [FromQuery] string name)
    {
        ProxiesMessage.StartProxyAsync(id, name).Wait();
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }

    [HttpGet("/api/server/get")]
    public IActionResult GetLaunchHttp()
    {
        var ip = Tools.GetLocalIpAddress();
        var proxies = ActiveGameAndProxies.GetAllProxies();
        var data = new JsonObject
        {
            ["ip"] = ip,
            ["proxies"] = JsonSerializer.SerializeToNode(proxies)
        };
        return Content(Code.ToJson(ErrorCode.Success, data), "application/json");
    }

    [HttpGet("/api/server/close")]
    public IActionResult CloseGame([FromQuery] int id)
    {
        ActiveGameAndProxies.CloseProxy(id);
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }
}