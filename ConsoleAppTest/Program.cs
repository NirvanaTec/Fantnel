// See https://aka.ms/new-console-template for more information

using Codexus.Cipher.Entities.WPFLauncher.NetGame;
using NirvanaPublic.Entities.Config;
using NirvanaPublic.Manager;
using NirvanaPublic.Message;
using NirvanaPublic.Utils;
using Serilog;

namespace ConsoleAppTest;

public static class Program
{
    public static void Main(string[] args)
    {
        Main1();
    }

    private static void Main1()
    {
        // Fantnel 日志初始化
        InitProgram.LogoInit();
        // Fantnel 初始化
        InitProgram.NelInit().Wait();

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

        while (InfoManager.GameUser == null || InfoManager.GameUser.UserId == null ||
               InfoManager.GameUser.AccessToken == null)
        {
            Log.Information("请输入cookie内容: ");
            cookie.Password = Console.ReadLine();
            Log.Information("登录中...");
            AccountMessage.Login(cookie);
        }

        Log.Information("登录成功! 用户ID: {UserId}", InfoManager.GetGameUser().UserId);

        // 创建角色名称
        const string serverId = "77114517833647104";
        string? gameName = null;

        while (gameName == null)
        {
            Log.Information("请输入角色名称: ");
            gameName = Console.ReadLine();
        }

        var list = ServerInfoMessage.GetUserName(serverId).Result;
        EntityGameCharacter? characters = null;

        foreach (var item in list)
            if (item.Name == gameName)
                characters = item;

        if (characters == null)
            InitProgram.GetServices().Wpf.CreateCharacter(InfoManager.GetGameUser().UserId,
                InfoManager.GetGameUser().AccessToken,
                serverId, gameName);

        Log.Information("角色名称: {GameName}", gameName);

        // 启动代理
        ProxiesMessage.StartProxyAsync(serverId, gameName).Wait();

        while (true) Task.Delay(1000);
    }
}