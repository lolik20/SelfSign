using SelfSign.Entities;

namespace SelfSign
{
    public static class FileService
    {
        public static async Task<string> AddFile(IFormFile file, Guid userId,Guid fileId)
        {
            string filesDirectory = Path.Combine(Environment.CurrentDirectory, $"Files\\{userId}");
            string filePath = "";
            Directory.CreateDirectory(filesDirectory);
            if (file.Length > 0)
            {
                filePath = Path.Combine(filesDirectory, $"{fileId}.jpg");
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            return filePath;
        }
        
        public static string GetCodeBase64(string phoneNumber, long id)
        {
            string filesDirectory = Path.Combine(Environment.CurrentDirectory, $"Files\\{phoneNumber}\\{id}.jpg");
            var bytes = File.ReadAllBytes(filesDirectory);
            string result = Convert.ToBase64String(bytes);
            return result;
        }
        public static byte[] GetDocument(string path)
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, path);
            var bytes = File.ReadAllBytes(filePath);
            return bytes;
        }
    }
}
