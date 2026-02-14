using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using NirvanaAPI.Manager;
using NirvanaAPI.Utils.CodeTools;
using Serilog;
using WPFLauncherApi.Entities;
using WPFLauncherApi.Entities.EntitiesWPFLauncher;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.Login;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.Minecraft;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.Minecraft.Mods;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameCharacters;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameDetails;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.GameMods;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.Texture;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameSkin;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.RentalGame;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.RentalGame.GameCharacters;
using WPFLauncherApi.Http;
using WPFLauncherApi.Utils;

namespace WPFLauncherApi.Protocol;

// ReSharper disable once InconsistentNaming
public static class WPFLauncher {
    
    private static readonly MgbSdk Sdk = new("x19");

    public static readonly JsonSerializerOptions DefaultOptions = new() {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /**
     * 查询服务器详细信息
     * @param gameId 服务器ID
     * @return 服务器详细信息
     */
    public static async Task<EntityQueryNetGameDetailItem> GetNetGameDetailByIdAsync(string gameId)
    {
        var response = await X19Extensions.Gateway.Api<EntityWPFLauncher<EntityQueryNetGameDetailItem>>(
            "/item-details/get_v2",
            new EntityQueryNetGameDetailRequest {
                ItemId = gameId
            }
        );
        return response == null ? throw new ErrorCodeException(ErrorCode.DetailError) : response.SafeEntity();
    }

    /**
     * 查询服务器地址
     * @param serverId 服务器ID
     * @return 服务器地址
     */
    public static async Task<EntityNetGameServerAddress> GetNetGameServerAddressAsync(string serverId)
    {
        var response = await X19Extensions.Gateway.Api<EntityWPFLauncher<EntityNetGameServerAddress>>(
            "/item-address/get",
            new EntityQueryNetGameDetailRequest {
                ItemId = serverId
            });
        return response == null ? throw new ErrorCodeException(ErrorCode.AddressError) : response.SafeEntity();
    }

    /**
     * 查询租赁服地址
     * @param serverId 服务器ID
     * @return 服务器地址
     */
    public static async Task<EntityRentalGameServerAddress> GetGameRentalAddressAsync(
        string serverId,
        string? pwd = null)
    {
        // 该接口存在问题，20%概率 因为缺少 引号 导致解析失败
        //  "entity_id": 4664453443934401593,
        var entity = await X19Extensions.Client.Api<EntityWPFLauncher<EntityRentalGameServerAddress>>(
            "/rental-server-world-enter/get", new EntityQueryRentalGameServerAddress {
                ServerId = serverId,
                Password = pwd ?? "none"
            });
        return entity == null ? throw new ErrorCodeException() : entity.SafeEntity();
    }

    /**
     * 获取服务器上的所有游戏角色
     * @param serverId 服务器ID
     * @return 服务器上的所有游戏角色
     */
    public static async Task<EntityGameCharacter[]> GetNetGameCharactersAsync(string gameId)
    {
        var response = await X19Extensions.Gateway.Api<EntitiesWPFLauncher<EntityGameCharacter>>(
            "/game-character/query/user-game-characters", new EntityQueryGameCharacters {
                GameId = gameId,
                UserId = InfoManager.GetUserId()
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
        var response = await X19Extensions.Gateway.Api<object>("/game-character",
            new EntityGameCharacter {
                GameId = gameId,
                UserId = InfoManager.GetUserId(),
                Name = roleName
            });
        if (response == null) throw new ErrorCodeException();
    }

    /**
    * 获取 租赁服 游戏角色
    */
    public static async Task<EntityRentalGamePlayerList[]> GetRentalGameRolesListAsync(string serverId)
    {
        var entity = await X19Extensions.Client.Api<EntitiesWPFLauncher<EntityRentalGamePlayerList>>(
            "/rental-server-player/query/search-by-user-server", new EntityQueryRentalGamePlayerList {
                ServerId = serverId,
                Offset = 0,
                Length = 10
            });
        return entity == null ? throw new ErrorCodeException() : entity.Data;
    }

    /**
     * 创建游戏角色
     * @param serverId 服务器ID
     * @param roleName 角色名称
     */
    public static async Task CreateCharacterRentalAsync(string serverId, string roleName)
    {
        var response = await X19Extensions.Gateway.Api<object>("/rental-server-player",
            new EntityAddRentalGameRole {
                ServerId = serverId,
                UserId = InfoManager.GetUserId(),
                Name = roleName,
                CreateTs = 555555,
                IsOnline = false,
                Status = 0
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
        var entity = await X19Extensions.Gateway.Api<EntitiesWPFLauncher<EntityNetGameItem>>("/item/query/available",
            new EntityNetGameRequest {
                AvailableMcVersions = [],
                ItemType = 1,
                Length = length,
                Offset = offset,
                MasterTypeId = "2",
                SecondaryTypeId = ""
            });
        return entity == null ? throw new ErrorCodeException() : entity.Data;
    }

    /**
     * 使用Cookie登录
     * @param cookie Cookie请求
     * @return 登录成功后的用户信息
     */
    public static async Task<EntityAuthenticationOtp> LoginWithCookieAsync(string cookie)
    {
        EntityX19CookieRequest? req;
        try {
            req = JsonSerializer.Deserialize<EntityX19CookieRequest>(cookie);
        } catch {
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
        if (entity.LoginChannel != "netease") {
            await Sdk.AuthSession(cookie.Json);
        }

        Log.Information("Login with Cookie...");
        var otp = await LoginOtpAsync(cookie);
        if (otp == null) {
            throw new ErrorCodeException(ErrorCode.LoginError);
        }

        var user = await AuthenticationOtpAsync(cookie, otp);
        // await InterConn.LoginStart();
        // await Task.Run((Func<Task>) (async () => await Http.GetAsync($"https://service.codexus.today/interconnection/report?id={user.EntityId}&token={user.Token}&version={this.MPay.GameVersion}")));
        return user ?? throw new ErrorCodeException(ErrorCode.LoginError);
    }

    /**
     * 获取登录OTP
     * @param cookieRequest Cookie数据
     * @return 登录OTP
     */
    private static async Task<EntityLoginOtp?> LoginOtpAsync(EntityX19CookieRequest cookieRequest)
    {
        var entity = await X19Extensions.Core.Api<EntityWPFLauncher<JsonElement?>>("/login-otp", cookieRequest);
        if (entity == null)
            throw new Exception("Failed to deserialize: login-otp");
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
        var authenticationDetail = new EntityAuthenticationDetail {
            Udid = "0000000000000000" + upper,
            AppVersion = X19.GameVersion,
            PayChannel = entityX19Cookie.AppChannel,
            Disk = upper
        };
        var entity = JsonSerializer.Deserialize<EntityWPFLauncher<EntityAuthenticationOtp>>(
            (ReadOnlySpan<byte>)HttpUtil.HttpDecrypt(await (await X19Extensions.Core.HttpWrapper.PostAsync(
                "/authentication-otp",
                HttpUtil.HttpEncrypt(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new EntityAuthenticationData {
                    SaData = JsonSerializer.Serialize(authenticationDetail, DefaultOptions),
                    AuthJson = cookieRequest.Json,
                    Version = new EntityAuthenticationVersion {
                        Version = X19.GameVersion
                    },
                    Aid = otp.Aid.ToString(),
                    OtpToken = otp.OtpToken,
                    LockTime = 0
                }, DefaultOptions))))).Content.ReadAsByteArrayAsync()));
        if (entity == null) {
            throw new ErrorCodeException(ErrorCode.LoginError);
        }

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
        var entity = await X19Extensions.Gateway.Api<EntitiesWPFLauncher<EntitySkin>>("/item/query/search-by-ids",
            new EntitySkinDetailsRequest {
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
        var entity = await X19Extensions.Gateway.Api<EntityWPFResponse>("/user-game-skin-multi",
            new {
                skin_settings = new List<EntitySkinSettings> {
                    new() {
                        ClientType = "java",
                        GameType = 9,
                        SkinId = entityId,
                        SkinMode = 0,
                        SkinType = 31
                    },
                    new() {
                        ClientType = "java",
                        GameType = 8,
                        SkinId = entityId,
                        SkinMode = 0,
                        SkinType = 31
                    },
                    new() {
                        ClientType = "java",
                        GameType = 2,
                        SkinId = entityId,
                        SkinMode = 0,
                        SkinType = 31
                    },
                    new() {
                        ClientType = "java",
                        GameType = 10,
                        SkinId = entityId,
                        SkinMode = 0,
                        SkinType = 31
                    },
                    new() {
                        ClientType = "java",
                        GameType = 7,
                        SkinId = entityId,
                        SkinMode = 0,
                        SkinType = 31
                    }
                }
            });
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
        var entity = await X19Extensions.Gateway.Api<EntitiesWPFLauncher<EntityQueryNetSkinItem>>(
            "/item/query/available",
            new EntityFreeSkinListRequest {
                IsHas = true,
                ItemType = 2,
                Length = length,
                MasterTypeId = 10,
                Offset = offset,
                PriceType = 3,
                SecondaryTypeId = 31
            });
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
    public static async Task<EntityQueryNetSkinItem[]> GetFreeSkinByNameAsync(string name, int offset = 0,
        int pageSize = 10)
    {
        var entity = await X19Extensions.Gateway.Api<EntitiesWPFLauncher<EntityQueryNetSkinItem>>(
            "/item/query/search-by-keyword",
            new EntityQuerySkinByNameRequest {
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
            });
        if (entity == null) throw new ErrorCodeException();
        return entity.Code != 0 ? throw new EntityX19Exception(entity.Message, entity) : entity.Data;
    }

    /**
     * 获取 白端基础 模组
     */
    public static async Task<EntityQuerySearchByGameResponse?> GetGameCoreModListAsync(
        EnumGameVersion gameVersion,
        bool isRental)
    {
        var entity = await X19Extensions.Gateway.Api<EntityWPFLauncher<EntityQuerySearchByGameResponse>>(
            "/game-auth-item-list/query/search-by-game", new EntityQuerySearchByGameRequest {
                McVersionId = (int)gameVersion,
                GameType = isRental ? 8 : 2
            });
        return entity == null ? throw new ErrorCodeException(ErrorCode.DetailError) : entity.Data;
    }

    /**
     * 获取 白端模组 详细信息
     */
    public static async Task<EntityComponentDownloadInfoResponse[]> GetGameCoreModDetailsListAsync(
        List<ulong> gameModList)
    {
        var entity = await X19Extensions.Gateway.Api<EntitiesWPFLauncher<EntityComponentDownloadInfoResponse>>(
            "/user-item-download-v2/get-list", new EntitySearchByIdsQuery {
                ItemIdList = gameModList
            });
        if (entity == null) throw new ErrorCodeException();
        return entity.Code != 0 ? throw new EntityX19Exception(entity.Message, entity) : entity.Data;
    }

    /**
    * 获取 白端服务器 模组
    */
    public static async Task<EntityComponentDownloadInfoResponse> GetNetGameComponentDownloadListAsync(
        string serverId)
    {
        return await GetNetGameComponentDownloadListAsync(InfoManager.GetUserId(), InfoManager.GetToken(), serverId);
    }

    /**
    * 获取 白端服务器 模组
    */
    public static async Task<EntityComponentDownloadInfoResponse> GetNetGameComponentDownloadListAsync(
        string? userId,
        string? userToken,
        string serverId)
    {
        var entity = await X19Extensions.Client.Api<EntityWPFLauncher<EntityComponentDownloadInfoResponse>>(
            "/user-item-download-v2", new EntitySearchByItemIdQuery {
                ItemId = serverId,
                Length = 0,
                Offset = 0
            },
            userId,
            userToken);
        return entity == null ? throw new ErrorCodeException(ErrorCode.DetailError) : entity.SafeEntity();
    }

    /**
     * 获取 白端依赖
     */
    public static async Task<EntityCoreLibResponse> GetMinecraftClientLibsAsync(
        EnumGameVersion? gameVersion = null)
    {
        uint gameVersionId = 0;
        if (gameVersion != null) gameVersionId = (uint)gameVersion.Value;
        var entity = await X19Extensions.Client.Api<EntityWPFLauncher<EntityCoreLibResponse>>(
            "/game-patch-info", new EntityMcDownloadVersion {
                McVersion = gameVersionId
            });
        return entity == null ? throw new ErrorCodeException() : entity.SafeEntity();
    }

    /**
     * 获取游戏皮肤
     */
    public static async Task<List<EntityUserGameTexture>> GetSkinListInGameAsync(
        string userId,
        string userToken,
        EntityUserGameTextureRequest userGame)
    {
        var entity = await X19Extensions.Gateway.Api<EntityWPFLauncher<EntityUserGameTexture[]>>(
            "/user-game-skin/query/search-by-type", userGame, userId, userToken);
        if (entity == null) throw new ErrorCodeException();
        return entity.Data == null ? throw new ErrorCodeException() : entity.Data.ToList();
    }

    /**
     * 获取 租赁服 列表
     */
    public static async Task<EntityRentalGame[]> GetRentalGameListAsync(
        int offset = 0)
    {
        var entity = await X19Extensions.Client.Api<EntitiesWPFLauncher<EntityRentalGame>>(
            "/rental-server/query/available-public-server", new EntityQueryRentalGame {
                Offset = offset,
                SortType = 0
            });
        return entity == null ? throw new ErrorCodeException() : entity.Data;
    }

    /**
     * 获取 租赁服 详细信息
     */
    public static async Task<EntityRentalGameDetails> GetRentalGameDetailsAsync(
        string entityId)
    {
        var entity = await X19Extensions.Client.Api<EntityWPFLauncher<EntityRentalGameDetails>>(
            "/rental-server-details/get", new EntityQueryRentalGameDetail {
                ServerId = entityId
            });
        return entity == null ? throw new ErrorCodeException() : entity.SafeEntity();
    }
}