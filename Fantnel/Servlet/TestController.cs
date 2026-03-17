using Microsoft.AspNetCore.Mvc;
using NirvanaAPI.Manager;
using NirvanaAPI.Utils.CodeTools;

namespace Fantnel.Servlet;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase {
    [HttpGet("/api/test")]
    public IActionResult Test()
    {
        foreach (var entity in InfoManager.GameAccountList) {
            entity.UserId = "1213";
            entity.Token = "1213";
        }

        return Ok(Code.ToJson(ErrorCode.Success));
    }
}