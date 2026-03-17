using Microsoft.AspNetCore.Mvc;
using Nirvana.Public.Entities.NEL;
using Nirvana.Public.Message;
using Nirvana.WPFLauncher.Protocol;
using NirvanaAPI.Utils.CodeTools;

namespace Fantnel.Servlet.GameController;

// game-rental
[ApiController]
[Route("[controller]")]
public class GameRentalController : ControllerBase {
    [HttpGet("/api/gamerental/get")]
    public IActionResult GetRentalGameListHttp([FromQuery] int offset, [FromQuery] int pageSize)
    {
        var entity = RentalGameMessage.GetServerList(offset, pageSize).Result;
        return Ok(Code.ToJson(ErrorCode.Success, entity));
    }

    [HttpGet("/api/gamerental/sort")]
    public IActionResult GetRentalGameSortHttp()
    {
        RentalGameMessage.SortServerList();
        return Ok(Code.ToJson(ErrorCode.Success));
    }

    [HttpGet("/api/gamerental/getlaunch")]
    public IActionResult GetRentalInfo([FromQuery] string id)
    {
        // 全部账号
        var accounts = AccountMessage.GetLoginAccountList();
        // 全部游戏角色
        var games = WPFLauncher.GetRentalGameRolesListAsync(id).Result;
        // 合并
        var text = new {
            accounts,
            games
        };
        return Ok(Code.ToJson(ErrorCode.Success, text));
    }

    [HttpGet("/api/gamerental/id")]
    public IActionResult GetIdServerHttp([FromQuery] string id)
    {
        var serverDetail = new EntityRentalDetail();
        serverDetail.Set(WPFLauncher.GetRentalGameDetailsAsync(id).Result);
        serverDetail.Set(WPFLauncher.GetGameRentalAddressAsync(id).Result);
        return Ok(Code.ToJson(ErrorCode.Success, serverDetail));
    }

    [HttpPost("/api/gamerental/createname")]
    public IActionResult CreateGameName([FromBody] EntityNewName name)
    {
        if (name.Id == null) throw new ErrorCodeException(ErrorCode.ServerInNot);
        if (name.Name == null) throw new ErrorCodeException(ErrorCode.NameInNot);
        WPFLauncher.CreateCharacterRentalAsync(name.Id, name.Name).Wait(); // 创建游戏角色
        RentalGameMessage.GetUserName(name.Id, name.Name).Wait(); // 防止缓存
        return Ok(Code.ToJson(ErrorCode.Success));
    }
}