using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;

namespace OpenSDK.Cipher;

public static class Rsa {
    public static AsymmetricKeyParameter LoadPublicKey(string base64PublicKey)
    {
        var keyBytes = Convert.FromBase64String(base64PublicKey);
        return PublicKeyFactory.CreateKey(keyBytes);
    }

    public static AsymmetricKeyParameter LoadPrivateKey(string base64PrivateKey)
    {
        var keyBytes = Convert.FromBase64String(base64PrivateKey);
        return PrivateKeyFactory.CreateKey(keyBytes);
    }

    public static byte[] RsaWithPkcs1(AsymmetricKeyParameter key, byte[] data, bool forEncryption)
    {
        var cipher = new Pkcs1Encoding(new RsaEngine());
        cipher.Init(forEncryption, key);

        var blockSize = cipher.GetInputBlockSize();
        using var ms = new MemoryStream();
        var offset = 0;
        while (offset < data.Length) {
            var chunkSize = Math.Min(blockSize, data.Length - offset);
            var chunk = cipher.ProcessBlock(data, offset, chunkSize);
            ms.Write(chunk, 0, chunk.Length);
            offset += chunkSize;
        }

        return ms.ToArray();
    }
}