using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Nirvana.Public;
using Nirvana.Public.Utils.ViewLogger;
using NirvanaAPI.Utils;
using NirvanaAPI.Utils.CodeTools;

namespace Fantnel.Servlet.OthersController;

[ApiController]
[Route("[controller]")]
public class FantController : ControllerBase {
    // 获取版本
    [HttpGet("/api/version")]
    public IActionResult GetVersion()
    {
        var version = new JsonObject {
            ["version"] = PublicProgram.Version,
            ["id"] = PublicProgram.VersionId,
            ["mode"] = PublicProgram.Mode,
            ["arch"] = PublicProgram.Arch
        };
        return Ok(Code.ToJson(ErrorCode.Success, version));
    }

    // 重启程序
    [HttpGet("/api/reboot")]
    public IActionResult Reboot()
    {
        // 重启程序
        Tools.Restart();
        return Ok(Code.ToJson(ErrorCode.Success));
    }

    // 关闭程序
    [HttpGet("/api/exit")]
    public IActionResult Exit()
    {
        Environment.Exit(0);
        return Ok(Code.ToJson(ErrorCode.Success));
    }

    // 关闭程序
    [HttpGet("/api/logs")]
    public IActionResult GetLogs()
    {
        return Ok(Code.ToJson(ErrorCode.Success, InMemorySink.GetLogs()));
    }
}