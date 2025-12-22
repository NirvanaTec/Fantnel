using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using NirvanaPublic.Message;
using NirvanaPublic.Utils;
using NirvanaPublic.Utils.ViewLogger;

namespace Fantnel.Servlet.GameController;

[ApiController]
[Route("[controller]")]
public class GameProxiesController : ControllerBase
{
    [HttpGet("/api/gameserver/launch")]
    public IActionResult LaunchGame([FromQuery] string id, [FromQuery] string name)
    {
        ProxiesMessage.StartProxyAsync(id, name).Wait();
        return Content(Code.ToJson(Code.ErrorCode.Success), "application/json");
    }

    [HttpGet("/api/server/get")]
    public IActionResult GetLaunchHttp()
    {
        var ip = Tools.GetLocalIpAddress();
        var proxies = ProxiesMessage.GetAllProxies();
        var data = new JsonObject
        {
            ["ip"] = ip,
            ["proxies"] = JsonSerializer.SerializeToNode(proxies)
        };
        return Content(Code.ToJson(Code.ErrorCode.Success, data), "application/json");
    }

    [HttpGet("/api/server/close")]
    public IActionResult CloseGame([FromQuery] int id)
    {
        ProxiesMessage.CloseProxy(id);
        return Content(Code.ToJson(Code.ErrorCode.Success), "application/json");
    }
}