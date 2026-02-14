using Microsoft.AspNetCore.Mvc;
using NirvanaAPI.Entities.Login;
using NirvanaAPI.Manager;
using NirvanaAPI.Utils;
using NirvanaAPI.Utils.CodeTools;
using NirvanaPublic.Entities.Login;
using NirvanaPublic.Message;
using Serilog;

namespace Fantnel.Servlet.GameController;

// game-accounts
[ApiController]
[Route("[controller]")]
public class GameAccountController : ControllerBase {
    
    [HttpGet("/api/gameaccount/get")]
    public IActionResult GetAccountHttp()
    {
        var entity = AccountMessage.GetAccountList();
        return Content(Code.ToJson(ErrorCode.Success, entity), "application/json");
    }
    
    [HttpGet("/api/gameaccount/available")]
    public IActionResult GetAccountAvailableHttp()
    {
        var entity = AccountMessage.GetLoginAccountList();
        return Content(Code.ToJson(ErrorCode.Success, entity), "application/json");
    }

    [HttpGet("/api/gameaccount/captcha4399")]
    public IActionResult GetCaptcha4399Http()
    {
        AccountMessage.UpdateCaptcha();
        return AccountMessage.Captcha4399Bytes == null
            ? throw new ErrorCodeException(ErrorCode.Failure)
            : File(AccountMessage.Captcha4399Bytes, "image/png");
    }

    [HttpPost("/api/gameaccount/captcha4399/verify")]
    public IActionResult VerifyCaptcha4399Http([FromBody] Entity4399CaptchaOk text)
    {
        AccountMessage.Captcha4399 = text.Captcha;
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }

    [HttpGet("/api/gameaccount/captcha4399/content")]
    public IActionResult GetCaptcha4399ContentHttp()
    {
        var captcha = AccountMessage.GetCaptcha4399Content().Result;
        return Content(Code.ToJson(ErrorCode.Success, captcha), "application/json");
    }

    [HttpGet("/api/gameaccount/select")]
    public IActionResult SelectAccount([FromQuery] int id)
    {
        try {
            AccountMessage.Login(id);
        } catch (Exception e) {
            Log.Error("登录失败: {Id}: {Message}", id, Tools.GetMessage(e));
            throw;
        }

        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }

    [HttpGet("/api/gameaccount/delete")]
    public IActionResult DeleteAccountHttp([FromQuery] int id)
    {
        try {
            AccountMessage.DeleteAccount(id);
        } catch (Exception e) {
            Log.Error("删除账号失败: {Id}: {Message}", id, Tools.GetMessage(e));
            throw;
        }

        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }

    [HttpPost("/api/gameaccount/save")]
    public IActionResult SaveAccountHttp([FromBody] EntityAccount account)
    {
        AccountMessage.SaveAccount(account);
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }

    [HttpPost("/api/gameaccount/update")]
    public IActionResult UpdateAccountHttp([FromBody] EntityAccount account)
    {
        AccountMessage.UpdateAccount(account);
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }

    [HttpGet("/api/gameaccount/switch")]
    public IActionResult SwitchAccountHttp([FromQuery] int id)
    {
        AccountMessage.SwitchAccount(id);
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }

    [HttpGet("/api/gameaccount/current")]
    public IActionResult GetGameAccountHttp()
    {
        return Content(Code.ToJson(ErrorCode.Success, InfoManager.GetGameAccount()), "application/json");
    }
}