using System.Text;
using OpenSDK.Cipher;
using OpenSDK.Extensions;

namespace OpenSDK.Entities.Yggdrasil;

public class UserProfile
{
    private static readonly byte[] TokenKey =
    [
        0xAC, 0x24, 0x9C, 0x69, 0xC7, 0x2C, 0xB3, 0xB4,
        0x4E, 0xC0, 0xCC, 0x6C, 0x54, 0x3A, 0x81, 0x95
    ];

    public required int UserId { get; set; }
    public required string UserToken { get; set; }

    public int GetAuthId()
    {
        return Skip32Cipher.Encrypt(UserId, "SaintSteve"u8.ToArray());
    }

    public byte[] GetAuthToken()
    {
        return Encoding.ASCII.GetBytes(UserToken).Xor(TokenKey);
    }
}