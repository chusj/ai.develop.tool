using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MultiAgentDevelop.Common.Helper
{
    /// <summary>
    /// AES 对称加密助手（AES-256-CBC + PKCS7）
    /// <para>
    /// 密文格式：Base64(IV[16 字节] + 密文)，每次加密使用随机 IV，同一明文两次加密结果不同。
    /// 密钥来源：appsettings.json 的 <c>Sms:SecretKey</c>，取 SHA256 摘要作为 32 字节密钥，
    /// 因此配置的密钥字符串长度不受限制。密钥缺失时回退到内置默认值（仅供开发环境）。
    /// </para>
    /// </summary>
    public static class AesHelper
    {
        /// <summary>
        /// 开发环境回退密钥。生产环境必须在 appsettings.json 配置 Sms:SecretKey。
        /// </summary>
        private const string FallbackSecret = "MultiAgentDevelop.Sms.DefaultSecret";

        private static readonly Lazy<byte[]> LazyKey = new Lazy<byte[]>(() =>
        {
            var secret = AppSettings.app(new[] { "Sms", "SecretKey" });
            if (string.IsNullOrWhiteSpace(secret))
            {
                secret = FallbackSecret;
            }
            return SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(secret));
        });

        /// <summary>
        /// 当前使用的密钥是否来自配置文件（false 表示在用内置回退密钥）
        /// </summary>
        public static bool HasConfiguredSecret =>
            !string.IsNullOrWhiteSpace(AppSettings.app(new[] { "Sms", "SecretKey" }));

        /// <summary>
        /// 加密。空值原样返回 null/空串，便于「留空不修改」的语义。
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <returns>Base64 密文</returns>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            using var aes = Aes.Create();
            aes.Key = LazyKey.Value;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var buffer = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, buffer, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, buffer, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(buffer);
        }

        /// <summary>
        /// 解密。
        /// </summary>
        /// <param name="cipherText">Encrypt 产出的 Base64 密文</param>
        /// <returns>明文；解密失败返回 null（不抛异常，避免脏数据导致接口 500）</returns>
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return cipherText;
            }

            try
            {
                var buffer = Convert.FromBase64String(cipherText);
                if (buffer.Length <= 16)
                {
                    return null;
                }

                using var aes = Aes.Create();
                aes.Key = LazyKey.Value;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var iv = new byte[16];
                Buffer.BlockCopy(buffer, 0, iv, 0, 16);
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream(buffer, 16, buffer.Length - 16);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs, Encoding.UTF8);
                return sr.ReadToEnd();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 加密并做幂等保护：若传入的已经是本算法产出的密文（解密成功），则原样返回。
        /// 用于「编辑时留空不修改」之外的二次保存场景。
        /// </summary>
        /// <param name="value">明文或密文</param>
        /// <returns>密文</returns>
        public static string EncryptIfPlain(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return Decrypt(value) != null ? value : Encrypt(value);
        }
    }
}
