namespace OpenSDK.Extensions;

public static class ByteArrayExtensions {
    extension(byte[] content) {
        public byte[] Xor(byte[] key)
        {
            if (content.Length != key.Length)
                throw new ArgumentException("Key length must be equal to content length.");

            var result = new byte[content.Length];
            for (var i = 0; i < content.Length; i++) result[i] = (byte)(content[i] ^ key[i]);

            return result;
        }

        public byte[] CombineWith(byte[] second)
        {
            return content.Concat(second).ToArray();
        }
    }
}