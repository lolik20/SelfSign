using Microsoft.Extensions.Configuration;
using SelfSign.BL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.BL.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly IConfigurationSection _configuration;
        private readonly ICryptoTransform _encryptor;
        private readonly ICryptoTransform _decryptor;
        public EncryptionService(IConfiguration configuration)

        {
            _configuration = configuration.GetSection("Encryption");
            Aes _aes = Aes.Create();
            _aes.Mode = CipherMode.CBC;
            _aes.KeySize = 128;
            _aes.BlockSize = 128;
            _aes.FeedbackSize = 128;
            _aes.Padding = PaddingMode.Zeros;
            _encryptor = _aes.CreateEncryptor(Encoding.ASCII.GetBytes(_configuration["Key"]), Encoding.ASCII.GetBytes(_configuration["Vector"]));
            _decryptor = _aes.CreateDecryptor(Encoding.ASCII.GetBytes(_configuration["Key"]), Encoding.ASCII.GetBytes(_configuration["Vector"]));
        }
        public byte[] Encrypt(byte[] bytes)
        {
            byte[] encrypted;

            using (MemoryStream mstream = new MemoryStream())

            using (CryptoStream csEncrypt = new CryptoStream(mstream, _encryptor, CryptoStreamMode.Write))
            {
                csEncrypt.Write(bytes, 0, bytes.Length);
                csEncrypt.FlushFinalBlock();
                encrypted = mstream.ToArray();

            }
            return encrypted;
        }
        public byte[] Decrypt(byte[] bytes)
        {

            byte[] decrypted;
            using (MemoryStream mstream = new MemoryStream())

            using (CryptoStream csDecrypt = new CryptoStream(mstream, _decryptor, CryptoStreamMode.Write))
            {
                csDecrypt.Write(bytes, 0, bytes.Length);
                csDecrypt.FlushFinalBlock();
                decrypted = mstream.ToArray();
            }




            return decrypted;
        }
    }
}
