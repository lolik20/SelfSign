using SelfSign.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.BL.Interfaces
{
    public interface IItMonitoringService
    {
        Task<Tuple<bool, string>> CreateRequest(object request);
        Task<Tuple<bool, string>> UpdateRequest(object request,string requestId);

        Task<bool> TwoFactor(string requestId, object request);
        Task<bool> UploadDocuments(string requestId, byte[] fileBytes, DocumentType documentType, string fileName, string fileExtension, string mimeType);
        Task<bool> Confirmation(string requestId);
        Task<dynamic> GetDocuments(string requestId);
        Task<byte[]> GetDocument(string requestId, DocumentType documentType);
        Task<bool> SimulateConfirmation(string requestId);
        Task<int> GetStatus(string requstId);
        Task<string> GetComment(string requestId);
    }
}
