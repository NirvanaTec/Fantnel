using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Serilog;
using WPFLauncherApi.Entities;
using WPFLauncherApi.Entities.EntitiesWPFLauncher;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.Login;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameCharacters;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameDetails;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameSkin;
using WPFLauncherApi.Http;
using WPFLauncherApi.Utils;
using WPFLauncherApi.Utils.CodeTools;
using HttpRequestOptions = WPFLauncherApi.Http.HttpRequestOptions;

namespace WPFLauncherApi.Protocol;

// ReSharper disable once InconsistentNaming
public static class WPFLauncher
{
    private static readonly HttpWrapper Core = new("https://x19obtcore.nie.netease.com:8443",
        (Action<HttpRequestOptions>)(builder => builder.UserAgent(GetUserAgent())));

    private static readonly MgbSdk Sdk = new("x19");

    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /**
     * 查询服务器详细信息
     * @param gameId 服务器ID
     * @return 服务器详细信息
     */
    public static async Task<EntityQueryNetGameDetailItem> QueryNetGameDetailByIdAsync(string gameId)
    {
        var response = await X19Extensions.Api<EntityWPFLauncher<EntityQueryNetGameDetailItem>>("/item-details/get_v2",
            new EntityQueryNetGameDetailRequest
            {
                ItemId = gameId
            }
        );
        return response == null ? throw new ErrorCodeException(ErrorCode.DetailError) : response.SafeEntity();
    }

    /**
     * 查询服务器地址
     * @param gameId 服务器ID
     * @return 服务器地址
     */
    public static async Task<EntityNetGameServerAddress> GetNetGameServerAddressAsync(string gameId)
    {
        var response = await X19Extensions.Api<EntityWPFLauncher<EntityNetGameServerAddress>>("/item-address/get",
            new EntityQueryNetGameDetailRequest
            {
                ItemId = gameId
            });
        return response == null ? throw new ErrorCodeException(ErrorCode.AddressError) : response.SafeEntity();
    }

    /**
     * 获取服务器上的所有游戏角色
     * @param serverId 服务器ID
     * @return 服务器上的所有游戏角色
     */
    public static async Task<EntityGameCharacter[]> QueryNetGameCharactersAsync(string gameId)
    {
        if (PublicProgram.User.UserId == null) throw new ErrorCodeException(ErrorCode.LogInNot);
        var response = await X19Extensions.Api<EntitiesWPFLauncher<EntityGameCharacter>>(
            "/game-character/query/user-game-characters", new EntityQueryGameCharacters
            {
                GameId = gameId,
                UserId = PublicProgram.User.UserId
            });
        return response == null ? throw new ErrorCodeException() : response.Data;
    }

    /**
     * 创建游戏角色
     * @param gameId 服务器ID
     * @param roleName 角色名称
     */
    public static async Task CreateCharacterAsync(string gameId, string roleName)
    {
        if (PublicProgram.User.UserId == null) throw new ErrorCodeException(ErrorCode.LogInNot);
        var response = await X19Extensions.Api<object>("/game-character/create",
            new EntityGameCharacter
            {
                GameId = gameId,
                UserId = PublicProgram.User.UserId,
                Name = roleName
            });
        if (response == null) throw new ErrorCodeException();
    }

    /**
     * 获取服务器列表
     * @param offset 偏移量
     * @param length 数量
     * @return 服务器列表
     */
    public static async Task<EntityNetGameItem[]> GetAvailableNetGamesAsync(int offset, int length)
    {
        var response = await X19Extensions.Api<EntitiesWPFLauncher<EntityNetGameItem>>("/item/query/available",
            new EntityNetGameRequest
            {
                AvailableMcVersions = [],
                ItemType = 1,
                Length = length,
                Offset = offset,
                MasterTypeId = "2",
                SecondaryTypeId = ""
            });
        return response == null ? throw new ErrorCodeException() : response.Data;
    }

    /**
     * 使用Cookie登录
     * @param cookie Cookie请求
     * @return 登录成功后的用户信息
     */
    public static async Task<EntityAuthenticationOtp> LoginWithCookieAsync(string cookie)
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
        return req == null ? throw new ErrorCodeException() : await LoginWithCookieAsync(req);
    }

    /**
     * 使用Cookie登录
     * @param cookie Cookie数据
     * @return 登录成功后的用户信息
     */
    private static async Task<EntityAuthenticationOtp> LoginWithCookieAsync(
        EntityX19CookieRequest cookie)
    {
        var entity = JsonSerializer.Deserialize<EntityX19Cookie>(cookie.Json);
        if (entity == null) throw new ErrorCodeException(ErrorCode.LoginError);
        if (entity.LoginChannel != "netease")
            await Sdk.AuthSession(cookie.Json);
        Log.Information("Login with Cookie...");
        var otp = await LoginOtpAsync(cookie);
        if (otp == null) throw new ErrorCodeException(ErrorCode.LoginError);
        var user = await AuthenticationOtpAsync(cookie, otp);
        if (user == null) throw new ErrorCodeException(ErrorCode.LoginError);
        PublicProgram.User.UserId = user.EntityId;
        PublicProgram.User.Token = user.Token;
        await InterConn.LoginStart();
        // await Task.Run((Func<Task>) (async () => await Http.GetAsync($"https://service.codexus.today/interconnection/report?id={user.EntityId}&token={user.Token}&version={this.MPay.GameVersion}")));
        return user;
    }

    /**
     * 获取登录OTP
     * @param cookieRequest Cookie数据
     * @return 登录OTP
     */
    private static async Task<EntityLoginOtp?> LoginOtpAsync(EntityX19CookieRequest cookieRequest)
    {
        var json = await (await Core.PostAsync("/login-otp", JsonSerializer.Serialize(cookieRequest, DefaultOptions)))
            .Content.ReadAsStringAsync();
        var entity = JsonSerializer.Deserialize<EntityWPFLauncher<JsonElement?>>(json);
        if (entity == null)
            throw new Exception("Failed to deserialize: " + json);
        if (entity.Code != 0 || !entity.Data.HasValue)
            throw new Exception("Failed to deserialize: " + entity.Message);
        return JsonSerializer.Deserialize<EntityLoginOtp>(entity.Data.Value.GetRawText());
    }

    /**
     * 使用OTP登录
     * @param cookieRequest Cookie数据
     * @param otp 登录OTP
     * @return 登录成功后的用户信息
     */
    private static async Task<EntityAuthenticationOtp?> AuthenticationOtpAsync(
        EntityX19CookieRequest cookieRequest,
        EntityLoginOtp otp)
    {
        var entityX19Cookie = JsonSerializer.Deserialize<EntityX19Cookie>(cookieRequest.Json);
        if (entityX19Cookie == null) throw new ErrorCodeException(ErrorCode.LoginError);
        var upper = StringGenerator.GenerateHexString(4).ToUpper();
        var authenticationDetail = new EntityAuthenticationDetail
        {
            Udid = "0000000000000000" + upper,
            AppVersion = X19.GameVersion,
            PayChannel = entityX19Cookie.AppChannel,
            Disk = upper
        };
        var entity = JsonSerializer.Deserialize<EntityWPFLauncher<EntityAuthenticationOtp>>(
            (ReadOnlySpan<byte>)HttpUtil.HttpDecrypt(await (await Core.PostAsync("/authentication-otp",
                HttpUtil.HttpEncrypt(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new EntityAuthenticationData
                {
                    SaData = JsonSerializer.Serialize(authenticationDetail, DefaultOptions),
                    AuthJson = cookieRequest.Json,
                    Version = new EntityAuthenticationVersion
                    {
                        Version = X19.GameVersion
                    },
                    Aid = otp.Aid.ToString(),
                    OtpToken = otp.OtpToken,
                    LockTime = 0
                }, DefaultOptions))))).Content.ReadAsByteArrayAsync()));
        if (entity == null) throw new ErrorCodeException(ErrorCode.LoginError);
        return entity.Code == 0 ? entity.Data : throw new EntityX19Exception(entity.Message, entity);
    }

    public static string GetUserAgent()
    {
        return "WPFLauncher/" + X19.GameVersion;
    }

    /**
     * 获取皮肤详情
     * @param skinList 皮肤ID列表
     * @return 皮肤详情
     */
    private static async Task<EntitySkin[]> GetSkinDetailsAsync(List<string> skinList)
    {
        var entity = await X19Extensions.Api<EntitiesWPFLauncher<EntitySkin>>("/item/query/search-by-ids",
            new EntitySkinDetailsRequest
            {
                ChannelId = 11,
                EntityIds = skinList,
                IsHas = true,
                WithPrice = true,
                WithTitleImage = true
            });
        return entity == null ? throw new ErrorCodeException(ErrorCode.NotFound) : entity.Data;
    }

    /**
     * 获取皮肤详情
     * @param skinId 皮肤ID
     * @return 皮肤详情
     */
    public static async Task<EntitySkin> GetSkinDetailsAsync(string skinId)
    {
        return (await GetSkinDetailsAsync([skinId]))[0];
    }

    /**
     * 获取皮肤的第一张图片
     * 来自详情页
     * @param entityId 皮肤ID
     * @return 图片URL
     */
    public static async Task<string> GetFirstImage(string entityId)
    {
        return (await GetSkinDetailsAsync(entityId)).TitleImageUrl;
    }

    /**
     * 设置皮肤
     * @param entityId 皮肤ID
     * @return 操作结果
     */
    public static async Task<EntityWPFResponse?> SetSkinAsync(string entityId)
    {
        var entity = await X19Extensions.Api<EntityWPFResponse>("/user-game-skin-multi", JsonSerializer.Serialize(new
        {
            skin_settings = new List<EntitySkinSettings>
            {
                new()
                {
                    ClientType = "java",
                    GameType = 9,
                    SkinId = entityId,
                    SkinMode = 0,
                    SkinType = 31
                },
                new()
                {
                    ClientType = "java",
                    GameType = 8,
                    SkinId = entityId,
                    SkinMode = 0,
                    SkinType = 31
                },
                new()
                {
                    ClientType = "java",
                    GameType = 2,
                    SkinId = entityId,
                    SkinMode = 0,
                    SkinType = 31
                },
                new()
                {
                    ClientType = "java",
                    GameType = 10,
                    SkinId = entityId,
                    SkinMode = 0,
                    SkinType = 31
                },
                new()
                {
                    ClientType = "java",
                    GameType = 7,
                    SkinId = entityId,
                    SkinMode = 0,
                    SkinType = 31
                }
            }
        }));
        if (entity == null) throw new ErrorCodeException();
        return entity.Code != 0 ? throw new EntityX19Exception(entity.Message, entity) : entity;
    }

    /**
     * 获取免费皮肤列表
     * @param offset 偏移量
     * @param length 数量
     * @return 皮肤列表
     */
    public static async Task<EntityQueryNetSkinItem[]> GetFreeSkinListAsync(
        int offset,
        int length = 20)
    {
        var entity = await X19Extensions.Api<EntitiesWPFLauncher<EntityQueryNetSkinItem>>("/item/query/available",
            JsonSerializer.Serialize(new EntityFreeSkinListRequest
            {
                IsHas = true,
                ItemType = 2,
                Length = length,
                MasterTypeId = 10,
                Offset = offset,
                PriceType = 3,
                SecondaryTypeId = 31
            }));
        if (entity == null) throw new ErrorCodeException();
        return entity.Code != 0 ? throw new EntityX19Exception(entity.Message, entity) : entity.Data;
    }

    /**
     * 查询免费皮肤列表
     * @param name 皮肤名称
     * @param offset 偏移量
     * @param pageSize 数量
     * @return 皮肤列表
     */
    public static async Task<EntityQueryNetSkinItem[]> QueryFreeSkinByNameAsync(string name, int offset = 0,
        int pageSize = 10)
    {
        var entity = await X19Extensions.Api<EntitiesWPFLauncher<EntityQueryNetSkinItem>>(
            "/item/query/search-by-keyword",
            JsonSerializer.Serialize(new EntityQuerySkinByNameRequest
            {
                IsHas = true,
                IsSync = 0,
                ItemType = 2,
                Keyword = name,
                Length = pageSize,
                MasterTypeId = 10,
                Offset = offset,
                PriceType = 3,
                SecondaryTypeId = "31",
                SortType = 1,
                Year = 0
            }));
        if (entity == null) throw new ErrorCodeException();
        return entity.Code != 0 ? throw new EntityX19Exception(entity.Message, entity) : entity.Data;
    }
}