using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.BL.Interfaces
{
    public interface IEncryptionService
    {
        public byte[] Decrypt(byte[] bytes);
        public byte[] Encrypt(byte[] bytes);
    }
}
