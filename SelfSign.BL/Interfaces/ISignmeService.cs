using Newtonsoft.Json.Linq;
using SelfSign.BL.Services;
using SelfSign.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.BL.Interfaces
{
    public interface ISignmeService
    {
        Task<Tuple<bool, JObject, string?>> Create(User user, string shortCladr);
        Task<bool> UploadDocument(string snils, string base64, DocumentType documentType, string fileName, string fileExtension);
        Task<PrecheckResponse> PreCheck(User user);
    }
}
