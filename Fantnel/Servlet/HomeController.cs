using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NirvanaPublic.Entities;

namespace Fantnel.Servlet;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    
    [HttpGet("/api/home")]
    public IActionResult HomeInfo()
    {
        return Content(JsonSerializer.Serialize(InfoManager.FantnelInfo), "application/json");
    }
    
    public static string GetIndexHtml()
    {
        // 获取运行目录路径
        var resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "static", "index.html");
        return System.IO.File.ReadAllText(resourcesPath);
    }
    
}
