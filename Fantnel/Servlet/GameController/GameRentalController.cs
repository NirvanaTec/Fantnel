using Microsoft.AspNetCore.Mvc;
using NirvanaPublic.Entities.NEL;
using NirvanaPublic.Message;
using WPFLauncherApi.Protocol;
using WPFLauncherApi.Utils.CodeTools;

namespace Fantnel.Servlet.GameController;

// game-rental
[ApiController]
[Route("[controller]")]
public class GameRentalController: ControllerBase
{
    [HttpGet("/api/gamerental/get")]
    public IActionResult GetRentalGameListHttp([FromQuery] int offset, [FromQuery] int pageSize)
    {
        var entity = RentalGameMessage.GetServerList(offset, pageSize).Result;
        return Content(Code.ToJson(ErrorCode.Success, entity), "application/json");
    }

    [HttpGet("/api/gamerental/sort")]
    public IActionResult GetRentalGameSortHttp()
    {
        RentalGameMessage.SortServerList();
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }
    
    [HttpGet("/api/gamerental/getlaunch")]
    public IActionResult GetRentalInfo([FromQuery] string id)
    {
        // 全部账号
        var accounts = AccountMessage.GetLoginAccountList();
        // 全部游戏角色
        var games = WPFLauncher.GetRentalGameRolesListAsync(id).Result;
        // 合并
        var text = new
        {
            accounts,
            games
        };
        return Content(Code.ToJson(ErrorCode.Success, text), "application/json");
    }

    [HttpGet("/api/gamerental/id")]
    public IActionResult GetIdServerHttp([FromQuery] string id)
    {
        var serverDetail = new EntityRentalDetail();
        serverDetail.Set(WPFLauncher.GetRentalGameDetailsAsync(id).Result);
        serverDetail.Set(WPFLauncher.GetGameRentalAddressAsync(id).Result);
        return Content(Code.ToJson(ErrorCode.Success, serverDetail), "application/json");
    }
    
    [HttpPost("/api/gamerental/createname")]
    public IActionResult CreateGameName([FromBody] EntityNewName name)
    {
        if (name.Id == null) throw new ErrorCodeException(ErrorCode.ServerInNot);
        if (name.Name == null) throw new ErrorCodeException(ErrorCode.NameInNot);
        WPFLauncher.CreateCharacterRentalAsync(name.Id, name.Name).Wait(); // 创建游戏角色
        RentalGameMessage.GetUserName(name.Id, name.Name).Wait(); // 防止缓存
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }
    
}