using Microsoft.AspNetCore.Mvc;
using NirvanaAPI.Entities.EntitiesNirvana;
using NirvanaAPI.Utils.CodeTools;
using NirvanaPublic.Message;

namespace Fantnel.Servlet.GameController;

// game-launch
[ApiController]
[Route("[controller]")]
public class GameLaunchController : ControllerBase {
    
    [HttpGet("/api/gamelaunch/launch")]
    public IActionResult LaunchGame([FromQuery] string id, [FromQuery] string name, [FromQuery] string mode = "net")
    {
        LaunchMessage.LaunchGame(id, name, mode).Wait();
        return Ok(Code.ToJson(ErrorCode.Success));
    }

    [HttpGet("/api/gamelaunch/get")]
    public IActionResult GetLauncherService()
    {
        var list = ActiveGameAndProxies.GetAllLaunchers();
        return Ok(Code.ToJson(ErrorCode.Success, list));
    }

    [HttpGet("/api/gamelaunch/close")]
    public IActionResult CloseGame([FromQuery] string id)
    {
        ActiveGameAndProxies.CloseGame(id);
        return Ok(Code.ToJson(ErrorCode.Success));
    }
}