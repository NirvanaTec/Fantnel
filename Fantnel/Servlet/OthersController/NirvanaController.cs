using Microsoft.AspNetCore.Mvc;
using NirvanaAPI;
using NirvanaAPI.Utils.CodeTools;
using NirvanaPublic.Manager;

namespace Fantnel.Servlet.OthersController;

[ApiController]
[Route("[controller]")]
public class NirvanaController : ControllerBase {
    // 登录账号
    [HttpGet("/api/nirvana/login")]
    public IActionResult Login(string account, string password)
    {
        NirvanaAccountManager.Login(account, password).Wait();
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }

    // 退出登录
    [HttpGet("/api/nirvana/logout")]
    public IActionResult Logout()
    {
        NirvanaConfig.Logout();
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }

    // 获取账号
    [HttpGet("/api/nirvana/account/get")]
    public IActionResult GetAccount()
    {
        var entity = NirvanaAccountManager.GetInfo().Result;
        return Content(Code.ToJson(ErrorCode.Success, entity), "application/json");
    }

    // 设置配置
    [HttpGet("/api/nirvana/set")]
    public IActionResult SetAccount(string mode, string? value)
    {
        switch (mode) {
            case "hideAccount":
                NirvanaConfig.SetHideAccount(value ?? "false");
                break;
            case "friendly":
                NirvanaAccountManager.SetFriendly(value ?? "false");
                break;
            case "chatEnable":
                NirvanaAccountManager.SetChatEnable(value ?? "true");
                break;
            case "chatTarget":
                NirvanaConfig.SetChatTarget(value ?? "true");
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
            case "chatPrefix":
                NirvanaConfig.SetChatPrefix(value ?? "§6§l涅槃科技 §8|§r ");
                break;
            case "autoLoginGame":
                NirvanaConfig.SetAutoLoginGame(value ?? "true");
                break;
            case "autoLoginGame4399":
                NirvanaConfig.SetAutoLoginGame4399(value ?? "true");
                break;
            case "autoLoginGame4399Com":
                NirvanaConfig.SetAutoLoginGame4399Com(value ?? "true");
                break;
            case "autoLoginGame163Email":
                NirvanaConfig.SetAutoLoginGame163Email(value ?? "true");
                break;
            case "autoLoginGameCookie":
                NirvanaConfig.SetAutoLoginGameCookie(value ?? "true");
                break;
        }
        return Content(Code.ToJson(ErrorCode.Success), "application/json");
    }

    // 获取配置
    [HttpGet("/api/nirvana/get")]
    public IActionResult SetAccount()
    {
        var config = NirvanaConfig.Config;
        return Content(Code.ToJson(ErrorCode.Success, config), "application/json");
    }
}