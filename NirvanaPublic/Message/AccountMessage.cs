using System.Text;
using System.Text.Json;
using Codexus.Cipher.Entities.WPFLauncher;
using Codexus.Cipher.Utils.Http;
using Codexus.Development.SDK.Entities;
using NirvanaPublic.Entities.Config;
using NirvanaPublic.Manager;
using NirvanaPublic.Utils;
using NirvanaPublic.Utils.ViewLogger;
using Serilog;

namespace NirvanaPublic.Message;

public static class AccountMessage
{
    // 默认 自动登录 已执行完成
    private static readonly List<string> IsDefaultLogin = [];

    private static string? _session4399Id; // 验证ID
    public static string? Captcha4399; // 验证内容
    public static byte[]? Captcha4399Bytes; // 验证码图片

    /**
     * Session 4399
     */
    public static void UpdateCaptcha()
    {
        var captchaId = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        captchaId = captchaId[..8];
        var http = new HttpWrapper("https://ptlogin.4399.com");
        var response = http.GetAsync("/ptlogin/captcha.do?captchaId=" + captchaId).Result;
        _session4399Id = captchaId;
        Captcha4399Bytes = response.Content.ReadAsByteArrayAsync().Result;
    }

    /**
     * 根据 账号Id 获取账号
     * @param id 账号Id
     * @return 账号实体
     */
    private static EntityAccount GetAccount(int id)
    {
        var entity = GetAccountList();
        foreach (var item in entity)
            if (item.Id == id)
                return item;
        throw new Code.ErrorCodeException(Code.ErrorCode.NotFound);
    }

    /**
     * 获取所有账号列表
     * @return 账号实体数组
     */
    public static EntityAccount[] GetAccountList()
    {
        return GetAccountList1().Item1;
    }

    /**
     * 获取所有账号列表 和 账号文件路径
     * @return 账号实体数组 和 账号文件路径
     */
    private static (EntityAccount[], string) GetAccountList1()
    {
        var (entity, path) = Tools.GetValueOrDefault<EntityAccount>("account.json");

        // 给 账号 赋值 Id
        var index = -1;
        foreach (var item in entity)
        {
            index++;
            item.Id = index;
            item.UserId = null;
            // 登录成功 同步 UserId
            if (InfoManager.GameAccount != null && InfoManager.GameAccount.Equals(item))
                // 避免 引用赋值，导致 后续登录 失败
                item.UserId = InfoManager.GameAccount.UserId;
        }

        DefaultLogin(entity);
        return (entity, path);
    }

    // 登录游戏账号
    public static void Login(int id)
    {
        Login(GetAccount(id));
    }

    // 登录游戏账号
    public static void Login(EntityAccount account)
    {
        lock (LockManager.LoginLock)
        {
            if (account.Password == null) throw new Code.ErrorCodeException(Code.ErrorCode.PasswordError);

            (EntityAuthenticationOtp?, string?) result = (null, null); // 登录结果

            switch (account.Type)
            {
                // cookie
                case "cookie":
                    result = GetCookieLogin(account.Password);
                    break;
                case "4399":
                {
                    if (_session4399Id == null || Captcha4399 == null)
                        throw new Code.ErrorCodeException(Code.ErrorCode.CaptchaNot);

                    if (account.Account == null) throw new Code.ErrorCodeException(Code.ErrorCode.AccountError);

                    var cookie = InitProgram.GetServices().N4399
                        .LoginWithPasswordAsync(account.Account, account.Password, _session4399Id, Captcha4399);
                    result = GetCookieLogin(cookie);
                    UpdateCaptcha();
                    break;
                }
            }

            //         case "163Email":
            //             var mpay = new UniSdkMPay(Projects.DesktopMinecraft, "2.1.0");
            //             await mpay.InitializeDeviceAsync();
            //             var user = await mpay.LoginWithEmailAsync(account.Account, account.Password);
            //             if (user == null) throw new Code.ErrorCodeException(Code.ErrorCode.EmailOrPasswordError);
            //             result = await NirvanaPublic.Services.X19.ContinueAsync(user, mpay.Device);
            //             break;


            // 登录完成

            if (result.Item1 == null || result.Item2 == null)
                throw new Code.ErrorCodeException(Code.ErrorCode.LoginError);
            // if (saveAccount) SaveAccount(account);

            if (result.Item1.EntityId.Length < 1) return;
            InfoManager.GameAccount = account;
            InfoManager.GameAccount.UserId = result.Item1.EntityId;
            InfoManager.GameUser = new EntityAvailableUser
            {
                UserId = result.Item1.EntityId,
                AccessToken = result.Item1.Token,
                LastLoginTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            Log.Information("登录成功! 用户ID: {UserId}", InfoManager.GetGameUser().UserId);
        }
    }

    // 获取 cookie 登录信息
    private static (EntityAuthenticationOtp, string) GetCookieLogin(string cookie)
    {
        EntityX19CookieRequest? req;
        try
        {
            req = JsonSerializer.Deserialize<EntityX19CookieRequest>(cookie);
        }
        catch
        {
            req = new EntityX19CookieRequest { Json = cookie };
        }

        return req == null
            ? throw new Code.ErrorCodeException(Code.ErrorCode.Failure)
            : InitProgram.GetServices().Wpf.LoginWithCookie(req);
    }

    // 保存账号到文件
    public static void UpdateAccount(EntityAccount account)
    {
        lock (LockManager.GameSaveAccountLock)
        {
            // 获取账号列表
            var (accountList, accountPath) = GetAccountList1();

            if (account.Id == null) throw new Code.ErrorCodeException(Code.ErrorCode.IdError);

            // 修改账号
            accountList[account.Id.Value] = account;

            foreach (var item in accountList)
            {
                item.Id = null;
                item.UserId = null;
            }

            // 创建目录
            var directory = Path.GetDirectoryName(accountPath);
            if (directory == null)
                throw new Code.ErrorCodeException(Code.ErrorCode.DirectoryCreateError);
            Directory.CreateDirectory(directory);

            // 写入文件
            File.WriteAllText(accountPath, JsonSerializer.Serialize(accountList), Encoding.UTF8);
        }

        // 自动登录账号
        AutoLogin(account);
    }

    // 保存账号到文件
    public static void SaveAccount(EntityAccount account)
    {
        lock (LockManager.GameSaveAccountLock)
        {
            // 获取账号列表
            var (accountList, accountPath) = GetAccountList1();

            foreach (var item in accountList)
            {
                item.Id = null;
                item.UserId = null;
            }

            // cookie 默认 假账号
            if (account.Account == null && account.Type == "cookie")
                // 10 位时间戳
                account.Account = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            // 增加账号
            accountList = accountList.Append(account).ToArray();

            // 创建目录
            var directory = Path.GetDirectoryName(accountPath);
            if (directory == null)
                throw new Code.ErrorCodeException(Code.ErrorCode.DirectoryCreateError);
            Directory.CreateDirectory(directory);

            // 写入文件
            File.WriteAllText(accountPath, JsonSerializer.Serialize(accountList), Encoding.UTF8);
        }

        // 自动登录账号
        AutoLogin(account);
    }

    // 自动登录账号
    private static void AutoLogin(EntityAccount account)
    {
        if (!InfoManager.IsNotLogin()) return;
        try
        {
            if (account.Type == "cookie") Login(account);
        }
        catch (Exception e)
        {
            IsDefaultLogin.Add(account.ToString());
            Log.Warning("自动登录失败: {account}: {Message}", account.Id, e.Message);
        }
    }

    // 删除账号到文件
    public static void DeleteAccount(int id)
    {
        lock (LockManager.GameSaveAccountLock)
        {
            // 获取账号列表
            var (accountList, accountPath) = GetAccountList1();

            // 删除账号
            accountList[id] = null!;

            // 创建目录
            var directory = Path.GetDirectoryName(accountPath);
            if (directory == null)
                throw new Code.ErrorCodeException(Code.ErrorCode.DirectoryCreateError);
            Directory.CreateDirectory(directory);

            // 写入文件
            File.WriteAllText(accountPath, JsonSerializer.Serialize(accountList), Encoding.UTF8);
        }
    }

    // 全自动执行默认登录
    public static void DefaultLogin(EntityAccount[] entity)
    {
        // 默认登录
        foreach (var item in entity)
        {
            if (!InfoManager.IsNotLogin()) break;
            var disabled = IsDefaultLogin.Any(defaultLogin => defaultLogin == item.ToString());

            if (disabled) continue;
            AutoLogin(item);
        }
    }
}