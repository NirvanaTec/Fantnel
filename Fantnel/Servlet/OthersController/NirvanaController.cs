using Microsoft.AspNetCore.Mvc;
using Nirvana.Public.Manager;
using NirvanaAPI;
using NirvanaAPI.Entities.EntitiesNirvana;
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
                NirvanaConfig.SetHideAccount(value ?? new EntityNirvanaConfig().HideAccount.ToString());
                break;
            case "chatEnable":
                NirvanaAccountManager.SetChatEnable(value ??  new EntityNirvanaConfig().ChatEnable.ToString());
                break;
            case "gameMemory":
                NirvanaConfig.SetGameMemory(value ?? new EntityNirvanaConfig().GameMemory.ToString());
                break;
            case "jvmArgs":
                NirvanaConfig.SetJvmArgs(value ?? new EntityNirvanaConfig().JvmArgs);
                break;
            case "gameArgs":
                NirvanaConfig.SetGameArgs(value ?? new EntityNirvanaConfig().GameArgs);
                break;
            case "autoLoginGame":
                NirvanaConfig.SetAutoLoginGame(value ?? new EntityNirvanaConfig().AutoLoginGame.ToString());
                break;
            case "autoLoginGame163Email":
                NirvanaConfig.SetAutoLoginGame163Email(value ?? new EntityNirvanaConfig().AutoLoginGame163Email.ToString());
                break;
            case "autoLoginGameCookie":
                NirvanaConfig.SetAutoLoginGameCookie(value ?? new EntityNirvanaConfig().AutoLoginGameCookie.ToString());
                break;
            case "useJavaW":
                NirvanaConfig.SetUseJavaW(value ?? new EntityNirvanaConfig().UseJavaW.ToString());
                break;
            case "autoUpdatePlugin":
                NirvanaConfig.SetAutoUpdatePlugin(value ?? new EntityNirvanaConfig().AutoUpdatePlugin.ToString());
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