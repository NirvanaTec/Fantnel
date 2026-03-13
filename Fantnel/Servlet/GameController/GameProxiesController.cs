using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using NirvanaAPI.Utils;
using NirvanaAPI.Utils.CodeTools;
using NirvanaPublic.Message;

namespace Fantnel.Servlet.GameController;

[ApiController]
[Route("[controller]")]
public class GameProxiesController : ControllerBase {
    
    [HttpGet("/api/gameserver/launch")]
    public IActionResult LaunchGame([FromQuery] string id, [FromQuery] string name, [FromQuery] string mode = "net")
    {
        var proxy = ProxiesMessage.StartProxyAsync(id, name, mode).Result;
        return Ok(Code.ToJson(ErrorCode.Success, proxy));
    }

    [HttpGet("/api/server/get")]
    public IActionResult GetLaunchHttp()
    {
        var ip = Tools.GetLocalIpAddress();
        var proxies = ActiveGameAndProxies.GetAllProxies();
        var data = new JsonObject {
            ["ip"] = ip,
            ["proxies"] = JsonSerializer.SerializeToNode(proxies)
        };
        return Ok(Code.ToJson(ErrorCode.Success, data));
    }

    [HttpGet("/api/server/close")]
    public IActionResult CloseGame([FromQuery] int id)
    {
        ActiveGameAndProxies.CloseProxy(id);
        return Ok(Code.ToJson(ErrorCode.Success));
    }
}