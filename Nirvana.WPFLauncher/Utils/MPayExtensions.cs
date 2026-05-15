using System;
using System.Security.Cryptography;
using System.Text;

namespace Nirvana.WPFLauncher.Utils;

public static class MPayExtensions {
    extension(byte[] inputBytes) {
        public string EncodeMd5()
        {
            return MD5.HashData(inputBytes).EncodeHex();
        }

        public string EncodeHex()
        {
            return Convert.ToHexString(inputBytes).Replace("-", "").ToLower();
        }
    }

    extension(string input) {
        public byte[] DecodeHex()
        {
            return Convert.FromHexString(input);
        }

        public byte[] EncodeAes(byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            using var encryptor = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(input);
            return encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
        }

        public string EncodeBase64()
        {
            return string.IsNullOrEmpty(input) ? string.Empty : Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }

        public string EncodeMd5()
        {
            return string.IsNullOrEmpty(input) ? string.Empty : Encoding.UTF8.GetBytes(input).EncodeMd5();
        }
    }
}