using System.IO.Hashing;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace OpenSDK.Cipher.Nirvana.Connection.ChaCha;

public sealed class ChaChaOfSalsa : ChaCha7539Engine
{
    public ChaChaOfSalsa(byte[] key, byte[] iv, bool encryption, int rounds = 8)
    {
        this.rounds = rounds;
        Init(encryption, new ParametersWithIV(new KeyParameter(key), iv));
    }

    public override string AlgorithmName => $"ChaCha{rounds}";

    public (byte, byte[]) UnpackMessage(byte[] data)
    {
        ProcessBytes(data, 0, data.Length, data, 0);
        var destination = new byte[4];
        Crc32.Hash(data.AsSpan(4, data.Length - 4), (Span<byte>)destination);
        for (var index = 0; index < 4; ++index)
            if (destination[index] != data[index])
                throw new Exception("Unpacking failed");
        var destinationArray = new byte[data.Length - 8];
        Array.Copy(data, 8, destinationArray, 0, destinationArray.Length);
        return (data[4], destinationArray);
    }

    public byte[] PackMessage(byte type, byte[] data)
    {
        var numArray = new byte[data.Length + 10];
        Array.Copy(BitConverter.GetBytes((short)(numArray.Length - 2)), 0, numArray, 0, 2);
        numArray[6] = type;
        numArray[7] = 136;
        numArray[8] = 136;
        numArray[9] = 136;
        Array.Copy(data, 0, numArray, 10, data.Length);
        Array.Copy(Crc32.Hash(numArray.AsSpan(6)), 0, numArray, 2, 4);
        ProcessBytes(numArray, 2, numArray.Length - 2, numArray, 2);
        return numArray;
    }
}