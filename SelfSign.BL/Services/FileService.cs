using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using Renci.SshNet.Common;
using SelfSign.BL.Interfaces;
using SelfSign.Common.Entities;

namespace SelfSign.BL.Services
{
    public class FileService : IFileService
    {
        private readonly SftpClient _client;
        private readonly IConfigurationSection _credentials;
        public FileService(IConfiguration configuration)

        {
            _credentials = configuration.GetSection("Ssh");
            _client = new SftpClient(_credentials["Host"], int.Parse(_credentials["Port"]), _credentials["Username"], _credentials["Password"]);
            _client.Connect();
        }
        public async Task<string> AddFile(IFormFile file, Guid userId, Guid fileId, string type)
        {

            string filesDirectory = $"/home/appuser/documents/{userId}";


            string directory = _client.WorkingDirectory;
            filesDirectory = filesDirectory.Replace("-", "");
            try
            {
                _client.CreateDirectory(filesDirectory);
            }
            catch (Exception)
            {

            }
            string filePath = "";
            if (file.Length > 0)
            {

                filePath = $"{filesDirectory}/{fileId.ToString().Replace("-", "")}.{type}";
                var fileBytes = FromFile(file);
                var stream = new MemoryStream();
                stream.Write(fileBytes, 0, fileBytes.Length);
                stream.Position = 0;
                _client.UploadFile(stream, filePath);
            }
            return filePath;


        }
        public async Task<string> AddFile(byte[] file, Guid userId, Guid fileId, string type)
        {

            string filesDirectory = $"/home/appuser/documents/{userId}";

            string directory = _client.WorkingDirectory;
            filesDirectory = filesDirectory.Replace("-", "");
            try
            {
                _client.CreateDirectory(filesDirectory);
            }
            catch (Exception)
            {

            }
            string filePath = "";
            if (file.Length > 0)
            {

                filePath = $"{filesDirectory}/{fileId.ToString().Replace("-", "")}.{type}";
                var stream = new MemoryStream();
                stream.Write(file, 0, file.Length);
                stream.Position = 0;
                _client.UploadFile(stream, filePath);
            }
            return filePath;


        }

        public byte[] GetDocument(string path)
        {


            var stream = new MemoryStream();
            _client.DownloadFile(path, stream);
            byte[] result = stream.ToArray();
            return result;


        }
        public byte[] FromFile(IFormFile formFile)
        {
            long length = formFile.Length;
            if (length < 0)
                return null;
            using var fileStream = formFile.OpenReadStream();
            byte[] bytes = new byte[length];
            fileStream.Read(bytes, 0, (int)formFile.Length);
            return bytes;
        }
    }
}
