using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using NirvanaAPI.Utils.CodeTools;

namespace Fantnel.Servlet;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase {
    [HttpGet("/api/test")]
    public IActionResult Test()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null) {
            var fileName = entryAssembly.Location;
            if (!string.IsNullOrEmpty(fileName))
                return Content(Code.ToJson(ErrorCode.Success, fileName), "application/json");
        }

        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }
}