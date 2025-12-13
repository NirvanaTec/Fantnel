// See https://aka.ms/new-console-template for more information

using Codexus.Cipher.Entities.WPFLauncher.NetGame;
using NirvanaPublic;
using NirvanaPublic.Entities;
using NirvanaPublic.Entities.Config;
using NirvanaPublic.Message;
using Serilog;

namespace ConsoleAppTest;

public static class Program
{
    public static void Main(string[] args)
    {
        // Fantnel 日志初始化
        PublicProgram.LogoInit();
        // Fantnel 初始化
        PublicProgram.NelInit().Wait();
        
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

        while (InfoManager.GameUser == null || InfoManager.GameUser.UserId == null || InfoManager.GameUser.AccessToken == null)
        {
            Log.Information("请输入cookie内容: ");
            cookie.Password = Console.ReadLine();
            Log.Information("登录中...");
            AccountMessage.Login(cookie);
        }
        
        Log.Information("登录成功! 用户ID: {UserId}", InfoManager.GameUser.UserId);
        
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
        {
            if (item.Name == gameName)
            {
                characters =  item;
            }
        }

        if (characters == null)
        {
            PublicProgram.Services?.Wpf.CreateCharacter(InfoManager.GameUser.UserId, InfoManager.GameUser.AccessToken,
                serverId, gameName);
        }
        
        Log.Information("角色名称: {GameName}", gameName);
        
        // 启动代理
        ProxiesMessage.StartProxyAsync(serverId, gameName).Wait();

        while (true)
        {
            Task.Delay(1000);
        }

    }
}