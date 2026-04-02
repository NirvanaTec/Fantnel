using Microsoft.AspNetCore.Mvc;
using Nirvana.Public.Manager;
using NirvanaAPI;
using NirvanaAPI.Utils.CodeTools;

namespace Fantnel.Servlet.OthersController;

[ApiController]
[Route("[controller]")]
public class NirvanaController : ControllerBase {
    // 登录账号
    [HttpGet("/api/nirvana/login")]
    public IActionResult Login(string account, string password)
    {
        NirvanaAccountManager.Login(account, password).Wait();
        return Ok(Code.ToJson(ErrorCode.Success));
    }

    // 退出登录
    [HttpGet("/api/nirvana/logout")]
    public IActionResult Logout()
    {
        NirvanaConfig.Logout();
        return Ok(Code.ToJson(ErrorCode.Success));
    }

    // 获取账号
    [HttpGet("/api/nirvana/account/get")]
    public IActionResult GetAccount()
    {
        var entity = NirvanaAccountManager.GetLoginInfo().Result;
        return Ok(Code.ToJson(ErrorCode.Success, entity));
    }

    // 设置配置
    [HttpGet("/api/nirvana/set")]
    public IActionResult SetAccount(string mode, string? value)
    {
        if ("gameMemory".Equals(mode, StringComparison.OrdinalIgnoreCase)) {
            NirvanaConfig.SetGameMemory(value);
        } else if ("chatEnable".Equals(mode, StringComparison.OrdinalIgnoreCase)) {
            NirvanaAccountManager.SetChatEnable(value);
        } else {
            NirvanaConfig.SetValue(mode, value);
        }

        return Ok(Code.ToJson(ErrorCode.Success));
    }

    // 获取配置
    [HttpGet("/api/nirvana/get")]
    public IActionResult SetAccount()
    {
        var config = NirvanaConfig.GetJsonObject();
        return Ok(Code.ToJson(ErrorCode.Success, config));
    }
}