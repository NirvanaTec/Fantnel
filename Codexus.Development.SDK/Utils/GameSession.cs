namespace Codexus.Development.SDK.Utils;

public class GameSession(string nickName, string userId, string userToken) {
    public string NickName { get; set; } = nickName;

    public string UserId { get; set; } = userId;

    public string UserToken { get; set; } = userToken;

    public override string ToString()
    {
        return $"{NickName},{UserId},{UserToken}";
    }
}