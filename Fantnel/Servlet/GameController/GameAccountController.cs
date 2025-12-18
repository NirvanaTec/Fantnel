using Microsoft.AspNetCore.Mvc;
using NirvanaPublic.Entities.Config;
using NirvanaPublic.Entities.Login;
using NirvanaPublic.Message;
using NirvanaPublic.Utils.ViewLogger;
using Serilog;

namespace Fantnel.Servlet.GameController;

// game-accounts
[ApiController]
[Route("[controller]")]
public class GameAccountController : ControllerBase
{
    [HttpGet("/api/gameaccount/get")]
    public IActionResult GetAccountHttp()
    {
        var entity = AccountMessage.GetAccountList();
        return Content(Code.ToJson(Code.ErrorCode.Success, entity), "application/json");
    }

    [HttpGet("/api/gameaccount/captcha4399")]
    public IActionResult GetCaptcha4399Http()
    {
        AccountMessage.UpdateCaptcha();
        return AccountMessage.Captcha4399Bytes == null
            ? throw new Code.ErrorCodeException(Code.ErrorCode.Failure)
            : File(AccountMessage.Captcha4399Bytes, "image/png");
    }

    [HttpPost("/api/gameaccount/captcha4399/verify")]
    public IActionResult VerifyCaptcha4399Http([FromBody] Entity4399CaptchaOk text)
    {
        AccountMessage.Captcha4399 = text.Captcha;
        return Content(Code.ToJson(Code.ErrorCode.Success), "application/json");
    }

    [HttpGet("/api/gameaccount/select")]
    public IActionResult SelectAccount([FromQuery] int id)
    {
        try
        {
            AccountMessage.Login(id);
        }
        catch (Exception e)
        {
            Log.Error("登录失败: {Id}: {Message}", id, e.Message);
            throw;
        }

        return Content(Code.ToJson(Code.ErrorCode.Success), "application/json");
    }

    [HttpGet("/api/gameaccount/delete")]
    public IActionResult DeleteAccountHttp([FromQuery] int id)
    {
        try
        {
            AccountMessage.DeleteAccount(id);
        }
        catch (Exception e)
        {
            Log.Error("删除账号失败: {Id}: {Message}", id, e.Message);
            throw;
        }

        return Content(Code.ToJson(Code.ErrorCode.Success), "application/json");
    }

    [HttpPost("/api/gameaccount/save")]
    public IActionResult SaveAccountHttp([FromBody] EntityAccount account)
    {
        AccountMessage.SaveAccount(account);
        return Content(Code.ToJson(Code.ErrorCode.Success), "application/json");
    }

    [HttpPost("/api/gameaccount/update")]
    public IActionResult UpdateAccountHttp([FromBody] EntityAccount account)
    {
        AccountMessage.UpdateAccount(account);
        return Content(Code.ToJson(Code.ErrorCode.Success), "application/json");
    }
}