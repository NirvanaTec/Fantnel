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
        var entity = NirvanaAccountManager.GetInfo().Result;
        return Ok(Code.ToJson(ErrorCode.Success, entity));
    }

    // 设置配置
    [HttpGet("/api/nirvana/set")]
    public IActionResult SetAccount(string mode, string? value)
    {
        switch (mode) {
            case "hideAccount":
                NirvanaConfig.SetHideAccount(value ?? "false");
                break;
            case "chatEnable":
                NirvanaAccountManager.SetChatEnable(value ?? "true");
                break;
            case "gameMemory":
                NirvanaConfig.SetGameMemory(value ?? "4096");
                break;
            case "jvmArgs":
                NirvanaConfig.SetJvmArgs(value);
                break;
            case "gameArgs":
                NirvanaConfig.SetGameArgs(value ?? string.Empty);
                break;
            case "autoLoginGame":
                NirvanaConfig.SetAutoLoginGame(value ?? "true");
                break;
            case "autoLoginGame163Email":
                NirvanaConfig.SetAutoLoginGame163Email(value ?? "true");
                break;
            case "autoLoginGameCookie":
                NirvanaConfig.SetAutoLoginGameCookie(value ?? "true");
                break;
            case "useJavaW":
                NirvanaConfig.SetUseJavaW(value ?? "true");
                break;
            case "autoUpdatePlugin":
                NirvanaConfig.SetAutoUpdatePlugin(value ?? "true");
                break;
        }

        return Ok(Code.ToJson(ErrorCode.Success));
    }

    // 获取配置
    [HttpGet("/api/nirvana/get")]
    public IActionResult SetAccount()
    {
        var config = NirvanaConfig.Config;
        return Ok(Code.ToJson(ErrorCode.Success, config));
    }
}