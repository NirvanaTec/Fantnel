using Microsoft.AspNetCore.Mvc;
using NirvanaPublic.Message;
using WPFLauncherApi.Utils.CodeTools;

namespace Fantnel.Servlet.GameController;

// game-launch
[ApiController]
[Route("[controller]")]
public class GameLaunchController : ControllerBase
{
    [HttpGet("/api/gamelaunch/launch")]
    public IActionResult LaunchGame([FromQuery] string id, [FromQuery] string name)
    {
        LaunchMessage.LaunchGame(id, name).Wait();
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }

    [HttpGet("/api/gamelaunch/get")]
    public IActionResult GetLauncherService()
    {
        var list = LaunchMessage.GetLauncherService();
        return Content(Code.ToJson(ErrorCode.Success, list), "application/json");
    }

    [HttpGet("/api/gamelaunch/close")]
    public IActionResult CloseGame([FromQuery] string id)
    {
        ActiveGameAndProxies.CloseGame(id);
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }
}