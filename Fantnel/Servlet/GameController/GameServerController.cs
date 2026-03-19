using Microsoft.AspNetCore.Mvc;
using Nirvana.Public.Entities.NEL;
using Nirvana.Public.Message;
using Nirvana.WPFLauncher.Protocol;
using NirvanaAPI.Utils.CodeTools;

namespace Fantnel.Servlet.GameController;

// servers
[ApiController]
[Route("[controller]")]
public class GameServerController : ControllerBase {
    [HttpGet("/api/gameserver/get")]
    public IActionResult GetServerHttp([FromQuery] int offset = 0, [FromQuery] int pageSize = 10, [FromQuery] string version = "")
    {
        var entity = ServersGameMessage.GetServerListTo(offset, pageSize, true, version).Result;
        return Ok(Code.ToJson(ErrorCode.Success, entity));
    }

    [HttpGet("/api/gameserver/id")]
    public IActionResult GetIdServerHttp([FromQuery] string id)
    {
        var serverDetail = new EntityServerDetail(id);
        return Ok(Code.ToJson(ErrorCode.Success, serverDetail));
    }

    [HttpGet("/api/gameserver/getlaunch")]
    public IActionResult GetServerInfo([FromQuery] string id)
    {
        // 全部账号
        var accounts = AccountMessage.GetLoginAccountList();
        // 全部游戏角色
        var games = NPFLauncher.GetNetGameCharactersAsync(id).Result;
        // 合并
        var text = new {
            accounts,
            games
        };
        return Ok(Code.ToJson(ErrorCode.Success, text));
    }

    [HttpPost("/api/gameserver/createname")]
    public IActionResult CreateGameName([FromBody] EntityNewName name)
    {
        if (name.Id == null) throw new ErrorCodeException(ErrorCode.ServerInNot);
        if (name.Name == null) throw new ErrorCodeException(ErrorCode.NameInNot);
        NPFLauncher.CreateCharacterAsync(name.Id, name.Name).Wait(); // 创建游戏角色
        ServersGameMessage.GetUserName(name.Id, name.Name).Wait(); // 防止缓存
        return Ok(Code.ToJson(ErrorCode.Success));
    }
}