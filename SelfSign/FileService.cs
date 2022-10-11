using Renci.SshNet;
using Renci.SshNet.Common;
using SelfSign.Entities;

namespace SelfSign
{
    public static class FileService
    {
        public static async Task<string> AddFile(IFormFile file, Guid userId,Guid fileId)
        {

            var host = "185.54.244.212";
            var port = 2022;
            var username = "appuser";
            var password = "luTZj3G7NkAF";
            string filesDirectory = $"/home/appuser/documents/{userId}";
            using (var client = new SftpClient(host, port, username, password))
            {
                client.Connect();
                 string directory =client.WorkingDirectory;
                filesDirectory = filesDirectory.Replace("-", "");
                try
                {
                    client.CreateDirectory(filesDirectory);
                }
                catch (Exception)
                {
                    
                }
                string filePath = "";
                if (file.Length > 0)
                {

                    filePath = $"{filesDirectory}/{fileId.ToString().Replace("-","")}.jpg";
                    var fileBytes = FromFile(file);
                    var stream = new MemoryStream();
                    stream.Write(fileBytes, 0, fileBytes.Length);
                    stream.Position = 0;
                    client.UploadFile(stream, filePath);
                }
                return filePath;

            }
        }
        
       
        public static byte[] GetDocument(string path)
        {
            var host = "185.54.244.212";
            var port = 2022;
            var username = "appuser";
            var password = "luTZj3G7NkAF";
            using (var client = new SftpClient(host, port, username, password))
            {
                client.Connect();
                var stream = new MemoryStream();
                client.DownloadFile(path, stream);
                byte[] result = stream.ToArray();
                return result;

            }
            return null;
        }
        private static byte[] FromFile(IFormFile formFile)
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
