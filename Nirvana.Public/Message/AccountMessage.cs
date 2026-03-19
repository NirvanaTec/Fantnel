using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Codexus.Cipher.Entities.MPay;
using Codexus.Cipher.Protocol;
using Nirvana.Public.Manager;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.Login;
using Nirvana.WPFLauncher.Protocol;
using NirvanaAPI.Entities;
using NirvanaAPI.Entities.Login;
using NirvanaAPI.Manager;
using NirvanaAPI.Utils;
using NirvanaAPI.Utils.CodeTools;
using Serilog;

namespace Nirvana.Public.Message;

public static class AccountMessage {
    // 保存/修改 游戏账号锁
    private static readonly Lock GameSaveAccountLock = new();

    // 登录游戏 锁
    private static readonly Lock LoginLock = new();


    // 默认 自动登录 已执行完成
    private static readonly List<EntityAccount> IsDefaultLogin = [];

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
        var http = new HttpClient();
        var response = http.GetAsync("https://ptlogin.4399.com/ptlogin/captcha.do?captchaId=" + captchaId).Result;
        _session4399Id = captchaId;
        Captcha4399Bytes = response.Content.ReadAsByteArrayAsync().Result;
    }

    /**
     * 根据 账号Id 获取账号
     * @param id 账号Id
     * @return 账号实体
     */
    private static EntityAccount GetAccount(int id, bool safeUserId = true)
    {
        var entity = GetAccountList(safeUserId);
        foreach (var item in entity) {
            if (item.Id == id) {
                return item;
            }
        }

        throw new ErrorCodeException(ErrorCode.NotFound);
    }

    /**
     * 切换账号 [安全]
     * @param id 账号Id
     */
    public static void SwitchAccount(int id)
    {
        var account = GetAccount(id);
        foreach (var gameAccount in InfoManager.GameAccountList.Where(gameAccount => gameAccount.Equals(account))) {
            InfoManager.SetGameAccount(gameAccount);
            break;
        }
    }

    // 强制切换账号
    public static void SwitchAccountToForce(int id)
    {
        InfoManager.SetGameAccount(GetAccount(id, false));
    }

    // 禁止默认登录
    public static void DisableDefaultLogin()
    {
        foreach (var gameAccount in GetAccountList1(false).Item1) {
            IsDefaultLogin.Add(gameAccount);
        }
    }

    /**
     * 获取登录成功后的账号列表
     * @return 账号实体数组
     */
    public static EntityAccount[] GetLoginAccountList()
    {
        var accountList = GetAccountList();
        return accountList.Where(account => InfoManager.GameAccountList.Any(gameAccount => gameAccount.Equals(account))).ToArray();
    }

    /**
     * 获取所有账号列表
     * @return 账号实体数组
     */
    public static EntityAccount[] GetAccountList(bool safeUserId = true)
    {
        return GetAccountList1(safeUserId: safeUserId).Item1;
    }

    /**
     * 获取所有账号列表 和 账号文件路径
     * @return 账号实体数组 和 账号文件路径
     */
    private static (EntityAccount[], string) GetAccountList1(bool defaultLogin = true, bool safeUserId = true)
    {
        var (entity, path) = Tools.GetValueOrDefaultList<EntityAccount>("account.json");

        // 给 账号 赋值 Id
        var index = -1;
        foreach (var item in entity) {
            index++;
            item.Id = index;
            // 登录成功 同步 UserId, Token
            foreach (var gameAccount in InfoManager.GameAccountList.Where(gameAccount => gameAccount.Equals(item))) {
                item.UserId = gameAccount.UserId;
                item.Token = gameAccount.Token;
                break;
            }
        }

        if (defaultLogin) {
            DefaultLogin(entity);
        }

        // 避免因配置加载的账号导致显示 UserId
        if (safeUserId) {
            foreach (var item in entity) {
                var flag = InfoManager.GameAccountList.Any(gameAccount => gameAccount.Equals(item));
                if (!flag) {
                    item.UserId = null;
                    item.Token = null;
                }
            }
        }

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
        lock (LoginLock) {
            if (account.Password == null) {
                throw new ErrorCodeException(ErrorCode.PasswordError);
            }

            EntityAuthenticationOtp? result = null; // 登录结果

            switch (account.Type) {
                case "cookie":
                    result = NPFLauncher.LoginWithCookieAsync(account.Password).Result;
                    break;
                case "4399" or "4399com" or "163Email" when account.Account == null:
                    throw new ErrorCodeException(ErrorCode.AccountError);
                case "4399" or "4399com" when _session4399Id == null || Captcha4399 == null:
                    throw new ErrorCodeException(ErrorCode.CaptchaNot);
                case "4399": {
                    var cookie = N4399.LoginWithPasswordAsync(account.Account, account.Password, _session4399Id, Captcha4399);
                    result = NPFLauncher.LoginWithCookieAsync(cookie).Result;
                    UpdateCaptcha();
                    break;
                }
                case "4399com": {
                    var cookie = NCom4399.LoginWithPasswordAsync(account.Account, account.Password, Captcha4399, _session4399Id);
                    result = NPFLauncher.LoginWithCookieAsync(cookie).Result;
                    UpdateCaptcha();
                    break;
                }
                case "163Email": {
                    var mpay = new MPay("aecfrxodyqaaaajp-g-x19", X19.GameVersion);
                    var mPayUser = mpay.LoginWithEmailAsync(account.Account, account.Password).Result;
                    var cookie = GenerateCookie(mPayUser, mpay.GetDevice());
                    result = NPFLauncher.LoginWithCookieAsync(cookie).Result;
                    break;
                }
            }

            // 登录完成
            if (result == null || result.EntityId.Length < 1) {
                throw new ErrorCodeException(ErrorCode.LoginError);
            }

            account.UserId = result.EntityId;
            account.Token = result.Token;
            InfoManager.AddAccount(account);

            // 登录成功后 保存账号
            SaveAccount();
            
            CacheManager.CacheServer();
        }
    }

    private static string GenerateCookie(
        EntityMPayUserResponse user,
        EntityDevice device)
    {
        return JsonSerializer.Serialize(new EntityX19Cookie {
            SdkUid = user.User.Id,
            SessionId = user.User.Token,
            Udid = Guid.NewGuid().ToString("N").ToUpper(),
            DeviceId = device.Id,
            AimInfo = "{\"aim\":\"127.0.0.1\",\"country\":\"CN\",\"tz\":\"+0800\",\"tzid\":\"\"}"
        }, NPFLauncher.DefaultOptions);
    }

    // 保存账号到文件
    public static void UpdateAccount(EntityAccount account, bool defaultLogin = true)
    {
        lock (GameSaveAccountLock) {
            // 获取账号列表
            var (accountList, accountPath) = GetAccountList1(defaultLogin);

            if (account.Id == null) throw new ErrorCodeException(ErrorCode.IdError);

            // 修改账号
            accountList[account.Id.Value] = account;

            // 写入文件
            File.WriteAllText(accountPath, JsonSerializer.Serialize(accountList), Encoding.UTF8);
        }

        // 自动登录账号
        AutoLogin(account);
    }

    // 保存账号到文件
    public static void SaveAccount(EntityAccount account)
    {
        lock (GameSaveAccountLock) {
            // 获取账号列表
            var (accountList, accountPath) = GetAccountList1();

            // cookie 默认 假账号
            if (account.Account == null && account.Type == "cookie") {
                // 10 位时间戳
                account.Account = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            }

            // 增加账号
            accountList = accountList.Append(account).ToArray();

            // 写入文件
            File.WriteAllText(accountPath, JsonSerializer.Serialize(accountList), Encoding.UTF8);
        }

        // 自动登录账号
        AutoLogin(account);
    }

    // 保存账号到文件
    private static void SaveAccount()
    {
        lock (GameSaveAccountLock) {
            // 获取账号列表
            var (accountList, accountPath) = GetAccountList1();
            // 写入文件
            File.WriteAllText(accountPath, JsonSerializer.Serialize(accountList), Encoding.UTF8);
        }
    }

    // 自动登录账号
    private static void AutoLogin(EntityAccount account, bool useConfig = false)
    {
        try {
            // 检查是否已登录过
            var disabled = IsDefaultLogin.Any(defaultLogin => defaultLogin.Equals(account));
            if (disabled) {
                return;
            }

            IsDefaultLogin.Add(account);

            Exception? exception = null;
            var success = false;

            if (account is { UserId: not null, Token: not null }) {
                try {
                    if (AutoUpdateAccount(account)) {
                        success = true;
                    }
                } catch (Exception e) {
                    exception = e;
                }
            }

            if (!success && account.Type is "cookie" or "163Email") {
                var isAutoLogin = true;
                if (useConfig) {
                    isAutoLogin = account.IsConfig();
                }

                if (isAutoLogin) {
                    Login(account);
                    success = true;
                    exception = null;
                }
            }

            if (!success && exception != null) {
                throw exception;
            }
        } catch (Exception e) {
            Log.Error("自动登录失败: {0}: {1}", account.Id, e.Message);
        }
    }

    // 真正的自动登录
    public static void AutoLogin1(EntityAccount account)
    {
        if ("4399".Equals(account.Type) || "4399com".Equals(account.Type)) {
            UpdateCaptcha();
            Captcha4399 = GetCaptcha4399Content().Result;
        }

        Login(account);
    }

    public static void AutoSwitchAccount(EntityAccount account)
    {
        foreach (var gameAccount in InfoManager.GameAccountList.Where(gameAccount => gameAccount.Equals(account))) {
            InfoManager.SetGameAccount(gameAccount);
            return;
        }

        AutoLogin1(account);
    }

    public static bool AutoUpdateAccount(EntityAccount account, Action? onFailure = null)
    {
        InfoManager.SetGameAccount(account);
        Exception? exception = null;

        try {
            var freeSkinCount = NPFLauncher.GetFreeSkinListAsync(0, 1).Result.Length;
            if (freeSkinCount > 0) {
                // 登录成功
                InfoManager.AddAccount(account);
                return true;
            }
        } catch (Exception e) {
            exception = e;
        }

        InfoManager.SetGameAccount(null);
        account.UserId = null;
        account.Token = null;
        UpdateAccount(account, false);

        onFailure?.Invoke();
        return exception == null ? false : throw exception;
    }

    // 删除账号到文件
    public static void DeleteAccount(int id)
    {
        lock (GameSaveAccountLock) {
            // 获取账号列表
            var (accountList, accountPath) = GetAccountList1();

            // 删除账号
            accountList[id] = null!;

            // 写入文件
            File.WriteAllText(accountPath, JsonSerializer.Serialize(accountList), Encoding.UTF8);
        }
    }

    // 全自动执行默认登录
    private static void DefaultLogin(EntityAccount[] entity)
    {
        // 默认登录
        foreach (var item in entity) {
            AutoLogin(item, true);
        }
    }

    /**
     * 获取4399验证码内容
     * @return 4399验证码内容
     */
    public static async Task<string> GetCaptcha4399Content()
    {
        if (Captcha4399Bytes == null) {
            throw new ErrorCodeException(ErrorCode.Failure);
        }

        var httpClient = new HttpClient();
        var response = await httpClient.PostAsync("http://110.42.70.32:13423/api/fantnel/captcha", new ByteArrayContent(Captcha4399Bytes));
        var resultJson = await response.Content.ReadAsStringAsync();
        var captcha = JsonSerializer.Deserialize<EntityResponse<string>>(resultJson);
        return captcha?.Data ?? throw new ErrorCodeException(ErrorCode.Failure);
    }
}