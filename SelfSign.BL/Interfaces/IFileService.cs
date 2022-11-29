using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.BL.Interfaces
{
    public interface IFileService
    {
        Task<string> AddFile(IFormFile file, Guid userId, Guid fileId, string type);
        Task<string> AddFile(byte[] file, Guid userId, Guid fileId, string type);
        byte[] GetDocument(string path);
        byte[] FromFile(IFormFile formFile);
    }
}
