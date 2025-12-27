using Microsoft.AspNetCore.Mvc;
using NirvanaPublic.Entities.NEL;
using NirvanaPublic.Message;
using NirvanaPublic.Utils.ViewLogger;

namespace Fantnel.Servlet.GameController;

[ApiController]
[Route("[controller]")]
public class GameSkinController : ControllerBase
{
    [HttpGet("/api/gameskin/get")]
    public IActionResult GetServerHttp([FromQuery] int offset = 0, [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null)
    {
        var entity = name == null
            ? SkinMessage.GetSkinList(offset, pageSize)
            : SkinMessage.GetSkinListByName(name, offset, pageSize);
        return Content(Code.ToJson(Code.ErrorCode.Success, entity), "application/json");
    }

    [HttpGet("/api/gameskin/detail")]
    public IActionResult GetSkinDetailHttp([FromQuery] string id)
    {
        var skinDetail = new EntitySkinDetail();
        skinDetail.Set(SkinMessage.GetSkinId(id));
        skinDetail.Set(SkinMessage.GetSkinDetails(id));
        return Content(Code.ToJson(Code.ErrorCode.Success, skinDetail), "application/json");
    }

    [HttpGet("/api/gameskin/set")]
    public IActionResult SetSkinHttp([FromQuery] string id)
    {
        SkinMessage.SetSkin(id);
        return Content(Code.ToJson(Code.ErrorCode.Success), "application/json");
    }
}