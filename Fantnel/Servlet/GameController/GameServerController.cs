using Microsoft.AspNetCore.Mvc;
using NirvanaPublic.Entities.NEL;
using NirvanaPublic.Message;
using NirvanaPublic.Utils.ViewLogger;

namespace Fantnel.Servlet.GameController;

// servers
[ApiController]
[Route("[controller]")]
public class GameServerController : ControllerBase
{
    [HttpGet("/api/gameserver/get")]
    public IActionResult GetServerHttp(int offset = 0, int pageSize = 10)
    {
        var entity = ServersGameMessage.GetServerList(offset, pageSize);
        return Content(Code.ToJson(Code.ErrorCode.Success, entity), "application/json");
    }

    [HttpGet("/api/gameserver/id")]
    public IActionResult GetIdServerHttp([FromQuery] string id)
    {
        var serverDetail = new EntityServerDetail();
        serverDetail.Set(ServersGameMessage.GetServerId(id));
        serverDetail.Set(ServerInfoMessage.GetServerId2(id).Result);
        serverDetail.Set(ServerInfoMessage.GetServerAddress(id).Result);
        return Content(Code.ToJson(Code.ErrorCode.Success, serverDetail), "application/json");
    }

    [HttpGet("/api/gameserver/getlaunch")]
    public IActionResult GetLaunchInfo([FromQuery] string id)
    {
        // 全部账号
        var accounts = AccountMessage.GetAccountList();
        // 全部游戏角色
        var games = ServerInfoMessage.GetUserName(id).Result;
        // 合并
        var text = new
        {
            accounts,
            games
        };
        return Content(Code.ToJson(Code.ErrorCode.Success, text), "application/json");
    }

    [HttpPost("/api/gameserver/createname")]
    public IActionResult CreateGameName([FromBody] EntityNewName name)
    {
        if (name.Id == null) throw new Code.ErrorCodeException(Code.ErrorCode.ServerInNot);
        if (name.Name == null) throw new Code.ErrorCodeException(Code.ErrorCode.NameInNot);
        // 创建游戏角色
        ServerInfoMessage.CreateCharacter(name.Id, name.Name);
        return Content(Code.ToJson(Code.ErrorCode.Success), "application/json");
    }

    [HttpGet("/api/gameserver/launch")]
    public IActionResult LaunchGame([FromQuery] string id, [FromQuery] string name)
    {
        ProxiesMessage.StartProxyAsync(id, name).Wait();
        return Content(Code.ToJson(Code.ErrorCode.Success), "application/json");
    }
}