using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace OpenSDK.Cipher;

public sealed class ChaChaPacker : ChaCha7539Engine {
    public ChaChaPacker(byte[] key, byte[] iv, bool encryption, int rounds = 8)
    {
        this.rounds = rounds;
        Init(encryption, new ParametersWithIV(new KeyParameter(key), iv));
    }

    public override string AlgorithmName => $"ChaCha{rounds}";
}