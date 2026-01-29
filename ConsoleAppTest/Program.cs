// See https://aka.ms/new-console-template for more information

using NirvanaPublic.Manager;
using NirvanaPublic.Message;
using NirvanaPublic.Utils;
using OpenSDK.Entities.Config;
using Serilog;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameCharacters;
using WPFLauncherApi.Protocol;

// Fantnel 日志初始化
InitProgram.LogoInit();
// Fantnel 初始化
InitProgram.NelInit1();

Log.Information("请输入cookie内容: ");

// 登录 cookie
var cookie = new EntityAccount
{
    Account = "admin123",
    Type = "cookie",
    Password = Console.ReadLine()
};

Log.Information("登录中...");
AccountMessage.Login(cookie);

while (InfoManager.GameAccount == null || InfoManager.GameAccount.UserId == null ||
       InfoManager.GameAccount.Token == null)
{
    Log.Information("请输入cookie内容: ");
    cookie.Password = Console.ReadLine();
    Log.Information("登录中...");
    AccountMessage.Login(cookie);
}

// 创建角色名称
const string serverId = "77114517833647104";
string? gameName = null;

while (gameName == null)
{
    Log.Information("请输入角色名称: ");
    gameName = Console.ReadLine();
}

var list = WPFLauncher.GetNetGameCharactersAsync(serverId).Result;
EntityGameCharacter? characters = null;

foreach (var item in list)
    if (item.Name == gameName)
        characters = item;

if (characters == null)
    WPFLauncher.CreateCharacterAsync(serverId, gameName).Wait();

Log.Information("角色名称: {GameName}", gameName);

// 启动代理
ProxiesMessage.StartProxyAsyncTo(serverId, gameName).Wait();

while (true) await Task.Delay(1000);