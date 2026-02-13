using System.Text;
using NirvanaAPI.Entities.Login;
using OpenSDK.Cipher;
using OpenSDK.Extensions;

namespace OpenSDK.Entities.Yggdrasil;

public class UserProfile(EntityUserInfo user) {
    private static readonly byte[] TokenKey = [
        0xAC, 0x24, 0x9C, 0x69, 0xC7, 0x2C, 0xB3, 0xB4,
        0x4E, 0xC0, 0xCC, 0x6C, 0x54, 0x3A, 0x81, 0x95
    ];

    public readonly EntityUserInfo User = user;

    public int GetUserId()
    {
        return int.Parse(User.GetUserId());
    }

    public string GetToken()
    {
        return User.GetToken();
    }

    public int GetAuthId()
    {
        return Skip32Cipher.Encrypt(GetUserId(), "SaintSteve"u8.ToArray());
    }

    public byte[] GetAuthToken()
    {
        return Encoding.ASCII.GetBytes(GetToken()).Xor(TokenKey);
    }
}