using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NirvanaPublic.Entities;

namespace Fantnel.Servlet;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    [HttpGet("/api/home")]
    public IActionResult Index()
    {
        return Content(JsonSerializer.Serialize(InfoManager.FantnelInfo), "application/json");
    }
}