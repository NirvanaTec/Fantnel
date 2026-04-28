using Microsoft.AspNetCore.Mvc;
using Nirvana.Public.Entities.Login;
using Nirvana.Public.Entities.Nirvana;
using Nirvana.Public.Message;
using NirvanaAPI.Entities.Login;
using NirvanaAPI.Manager;
using NirvanaAPI.Utils;
using NirvanaAPI.Utils.CodeTools;
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
        return Ok(Code.ToJson(ErrorCode.Success, entity));
    }

    [HttpGet("/api/gameaccount/available")]
    public IActionResult GetAccountAvailableHttp()
    {
        var entity = AccountMessage.GetLoginAccountList();
        return Ok(Code.ToJson(ErrorCode.Success, entity));
    }

    [HttpGet("/api/gameaccount/captcha4399")]
    public IActionResult GetCaptcha4399Http()
    {
        AccountMessage.UpdateCaptcha();
        return AccountMessage.Captcha4399Bytes == null ? throw new ErrorCodeException(ErrorCode.Failure) : File(AccountMessage.Captcha4399Bytes, "image/png");
    }

    [HttpPost("/api/gameaccount/captcha4399/verify")]
    public IActionResult VerifyCaptcha4399Http([FromBody] Entity4399CaptchaOk text)
    {
        AccountMessage.Captcha4399 = text.Captcha;
        return Ok(Code.ToJson(ErrorCode.Success));
    }

    [HttpGet("/api/gameaccount/captcha4399/content")]
    public IActionResult GetCaptcha4399ContentHttp()
    {
        var captcha = AccountMessage.GetCaptcha4399Content().Result;
        return Ok(Code.ToJson(ErrorCode.Success, captcha));
    }

    [HttpGet("/api/gameaccount/select")]
    public IActionResult SelectAccount([FromQuery] int id)
    {
        try {
            AccountMessage.Login(id);
        } catch (Exception e) {
            Log.Error("登录失败: {0}: {1}", id, Tools.GetMessage(e));
            throw;
        }

        return Ok(Code.ToJson(ErrorCode.Success));
    }

    [HttpPost("/api/gameaccount/autoLogin")]
    public IActionResult AutoLoginHttp([FromBody] EntityAccount account)
    {
        try {
            AccountMessage.AutoLogin1(account);
            return Ok(Code.ToJson(ErrorCode.Success, account.Id));
        } catch (Exception e) {
            Log.Error("自动登录失败: {0}: {1}", account, Tools.GetMessage(e));
            throw;
        }
    }

    [HttpGet("/api/gameaccount/delete")]
    public IActionResult DeleteAccountHttp([FromQuery] int id)
    {
        try {
            AccountMessage.DeleteAccount(id);
        } catch (Exception e) {
            Log.Error("删除账号失败: {0}: {1}", id, Tools.GetMessage(e));
            throw;
        }

        return Ok(Code.ToJson(ErrorCode.Success));
    }

    [HttpPost("/api/gameaccount/save")]
    public IActionResult SaveAccountHttp([FromBody] EntityAccount account)
    {
        AccountMessage.SaveAccount(account);
        return Ok(Code.ToJson(ErrorCode.Success));
    }

    [HttpPost("/api/gameaccount/update")]
    public IActionResult UpdateAccountHttp([FromBody] EntityAccount account)
    {
        AccountMessage.UpdateAccount(account);
        return Ok(Code.ToJson(ErrorCode.Success));
    }

    [HttpGet("/api/gameaccount/switch")]
    public IActionResult SwitchAccountHttp([FromQuery] int id)
    {
        AccountMessage.SwitchAccount(id);
        return Ok(Code.ToJson(ErrorCode.Success));
    }

    [HttpPost("/api/gameaccount/autoswitch")]
    public IActionResult AutoSwitchAccountHttp([FromBody] EntityAccount account)
    {
        AccountMessage.AutoSwitchAccount(account);
        return Ok(Code.ToJson(ErrorCode.Success));
    }

    [HttpGet("/api/gameaccount/current")]
    public IActionResult GetGameAccountHttp()
    {
        return Ok(Code.ToJson(ErrorCode.Success, InfoManager.GetGameAccount()));
    }

    [HttpPost("/api/gameaccount/random")]
    public IActionResult RandomAccountHttp([FromBody] EntityGeeTest captcha)
    {
        AccountMessage.RandomAccount(captcha).Wait();
        return Ok(Code.ToJson(ErrorCode.Success));
    }
}