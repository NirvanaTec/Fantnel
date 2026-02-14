using NirvanaAPI.Entities;
using NirvanaAPI.Entities.Login;
using NirvanaAPI.Utils.CodeTools;
using Serilog;

namespace NirvanaAPI.Manager;

public static class InfoManager {
    
    // 登录成功后的游戏账号列表
    public static readonly List<EntityAccount> GameAccountList = [];

    // 涅槃 服务器 信息
    public static EntityInfo? FantnelInfo;

    private static EntityAccount? _gameAccount;

    public static void AddAccount(EntityAccount account)
    {
        Log.Information("登录成功! 用户ID: {UserId}", account.UserId);
        // 账号已存在
        foreach (var gameAccount in GameAccountList.Where(gameAccount => gameAccount.Equals(account))) {
            gameAccount.Update(account);
            return;
        }

        GameAccountList.Add(account);
    }

    public static void SetGameAccount(EntityAccount? account)
    {
        if (account == null) {
            _gameAccount = account;
            return;
        }
        // 排序变成第0个
        foreach (var gameAccount in GameAccountList.Where(gameAccount => gameAccount.Equals(account))) {
            GameAccountList.Remove(gameAccount);
            GameAccountList.Insert(0, gameAccount);
            _gameAccount = null;
           return;
        }
        _gameAccount = account;
    }
    
    // 游戏账号
    public static EntityAccount GetGameAccount()
    {
        if (_gameAccount != null) {
            return _gameAccount;
        }

        foreach (var gameAccount in GameAccountList.Where(gameAccount => gameAccount.IsNotNuLl())) {
            return gameAccount;
        }

        throw new ErrorCodeException(ErrorCode.LogInNot);
    }

    public static string GetUserId()
    {
        return GetGameAccount().GetUserId();
    }

    public static string GetToken()
    {
        return GetGameAccount().GetToken();
    }
}