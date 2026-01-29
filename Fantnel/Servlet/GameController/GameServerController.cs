using Microsoft.AspNetCore.Mvc;
using NirvanaPublic.Entities.NEL;
using NirvanaPublic.Message;
using WPFLauncherApi.Protocol;
using WPFLauncherApi.Utils.CodeTools;

namespace Fantnel.Servlet.GameController;

// servers
[ApiController]
[Route("[controller]")]
public class GameServerController : ControllerBase
{
    [HttpGet("/api/gameserver/get")]
    public IActionResult GetServerHttp([FromQuery] int offset = 0, [FromQuery] int pageSize = 10)
    {
        var entity = ServersGameMessage.GetServerList(offset, pageSize).Result;
        return Content(Code.ToJson(ErrorCode.Success, entity), "application/json");
    }

    [HttpGet("/api/gameserver/id")]
    public IActionResult GetIdServerHttp([FromQuery] string id)
    {
        var serverDetail = new EntityServerDetail();
        serverDetail.Set(ServersGameMessage.GetServerById(id));
        serverDetail.Set(WPFLauncher.GetNetGameDetailByIdAsync(id).Result);
        serverDetail.Set(WPFLauncher.GetNetGameServerAddressAsync(id).Result);
        return Content(Code.ToJson(ErrorCode.Success, serverDetail), "application/json");
    }

    [HttpGet("/api/gameserver/getlaunch")]
    public IActionResult GetServerInfo([FromQuery] string id)
    {
        // 全部账号
        var accounts = AccountMessage.GetLoginAccountList();
        // 全部游戏角色
        var games = WPFLauncher.GetNetGameCharactersAsync(id).Result;
        // 合并
        var text = new
        {
            accounts,
            games
        };
        return Content(Code.ToJson(ErrorCode.Success, text), "application/json");
    }

    [HttpPost("/api/gameserver/createname")]
    public IActionResult CreateGameName([FromBody] EntityNewName name)
    {
        if (name.Id == null) throw new ErrorCodeException(ErrorCode.ServerInNot);
        if (name.Name == null) throw new ErrorCodeException(ErrorCode.NameInNot);
        WPFLauncher.CreateCharacterAsync(name.Id, name.Name).Wait(); // 创建游戏角色
        ServersGameMessage.GetUserName(name.Id, name.Name).Wait(); // 防止缓存
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }
    
}