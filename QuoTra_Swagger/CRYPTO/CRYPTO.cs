using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

namespace QuoTra.CRYPTO
{
    public class CRYPTO
    {
        // hashのパラメーター
        private const int saltSize = 32;      // ソルトのサイズ
        private const int hashSize = 32;      // ハッシュサイズ
        private const int iteration = 10198;  // ストレッチングの回数
        public const string pepperTxt = "(´・ω・｀)(`・ω・´)";

        // hash 塩の生成                                      
        public static byte[] CreateSalt()
        {
            var bytes = new byte[saltSize];
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(bytes);
            }
            return bytes;
        }

        // hash化
        public static byte[] CreatePBKDF2Hash(string password, byte[] salt)
        {
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, iteration))
            {
                return rfc2898DeriveBytes.GetBytes(hashSize);
            }
        }

        // AESのパラメーター
        private const int BLOCK_SIZE = 128;   // 128bit 固定
        private const int KEY_SIZE = 256;     // 128/192/256bit から選択
        private const string commonkey = "a50875c831805f22b3f13efb7a34fbcc94c54a8b4a78d42f853a43b54fba54c5";

        private static AesCryptoServiceProvider csp = new AesCryptoServiceProvider()
        {
            BlockSize = BLOCK_SIZE,
            KeySize = KEY_SIZE,
            Mode = CipherMode.CBC,
            Padding = PaddingMode.PKCS7,
            Key = Convert.FromHexString(commonkey)
        };

        // IV initial vector 生成
        public static string getIV()
        {
            csp.GenerateIV();
            string iv = Convert.ToBase64String(csp.IV);
            return iv;
        }

        // AES 暗号化
        public string aesEncrypt(string plainText, string iv)
        {
            string cipherText = string.Empty;
            csp.IV = Convert.FromBase64String(iv);

            ICryptoTransform encryptor = csp.CreateEncryptor(csp.Key, csp.IV);
            using (MemoryStream outms = new MemoryStream())
            { 
                using (CryptoStream cs = new CryptoStream(outms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter writer = new StreamWriter(cs))
                    {
                        writer.Write(plainText);
                    }
                    cipherText = Convert.ToBase64String(outms.ToArray());
                }         
            }
            return cipherText;
        }

        // AES 復号化
        public string aesDecrypt(string cipherText, string iv)
        {
            string plainText = string.Empty;
            csp.IV = Convert.FromBase64String(iv);

            ICryptoTransform decryptor = csp.CreateDecryptor(csp.Key, csp.IV);

            using (MemoryStream inms = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (CryptoStream cs = new CryptoStream(inms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(cs))
                    {
                        plainText = reader.ReadToEnd();
                        // ここでエラー「Padding is invalid and cannot be removed.」が出る場合は
                        // 暗号文が正しく暗号化されているか確認する
                    }
                }
            }
            return plainText;
        }
    }

    public static class Password
    {
        private static readonly char[] Punctuations = "!@#$%^&*()_-+[{]}:>|/?".ToCharArray();
        public static string Generate(int length, int numberOfNonAlphanumericCharacters)
        {
            if (length < 1 || length > 128)
            {
                throw new ArgumentException("length");
            }

            if (numberOfNonAlphanumericCharacters > length || numberOfNonAlphanumericCharacters < 0)
            {
                throw new ArgumentException("numberOfNonAlphanumericCharacters");
            }

            using (var rng = RandomNumberGenerator.Create())
            {
                var byteBuffer = new byte[length];

                rng.GetBytes(byteBuffer);

                var count = 0;
                var characterBuffer = new char[length];

                for (var iter = 0; iter < length; iter++)
                {
                    var i = byteBuffer[iter] % 87;

                    if (i < 10)
                    {
                        characterBuffer[iter] = (char)('0' + i);
                    }
                    else if (i < 36)
                    {
                        characterBuffer[iter] = (char)('A' + i - 10);
                    }
                    else if (i < 62)
                    {
                        characterBuffer[iter] = (char)('a' + i - 36);
                    }
                    else
                    {
                        characterBuffer[iter] = Punctuations[GetRandomInt(rng, Punctuations.Length)];
                        count++;
                    }
                }

                if (count >= numberOfNonAlphanumericCharacters)
                {
                    return new string(characterBuffer);
                }

                int j;

                for (j = 0; j < numberOfNonAlphanumericCharacters - count; j++)
                {
                    int k;
                    do
                    {
                        k = GetRandomInt(rng, length);
                    }
                    while (!char.IsLetterOrDigit(characterBuffer[k]));

                    characterBuffer[k] = Punctuations[GetRandomInt(rng, Punctuations.Length)];
                }

                return new string(characterBuffer);
            }
        }

        private static int GetRandomInt(RandomNumberGenerator randomGenerator)
        {
            var buffer = new byte[4];
            randomGenerator.GetBytes(buffer);

            return BitConverter.ToInt32(buffer);
        }
        private static int GetRandomInt(RandomNumberGenerator randomGenerator, int maxInput)
        {
            return Math.Abs(GetRandomInt(randomGenerator) % maxInput);
        }
    }

}
