using ImageMagick;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SelfSign.BL.Interfaces;
using SelfSign.BL.Services;
using SelfSign.Common.RequestModels;
using SelfSign.Common.ResponseModels;
using SelfSign.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.BL.Commands
{
    public class PassportUploadCommand : IRequestHandler<PassportUploadRequest, PassportUploadResponse>
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IFileService _fileService;
        private readonly IHistoryService _historyService;
        public PassportUploadCommand(ApplicationContext context, IConfiguration configuration, HttpClient httpClient, IFileService fileService, IHistoryService historyService)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClient;
            _fileService = fileService;
            _historyService = historyService;
        }

        public async Task<PassportUploadResponse> Handle(PassportUploadRequest request, CancellationToken cancellationToken)
        {
            var user = _context.Users.Include(x => x.Requests.OrderByDescending(x => x.Created)).ThenInclude(x => x.Documents).FirstOrDefault(x => x.Id == request.Id);
            if (user == null)
            {
                return new PassportUploadResponse { IsSuccess = false };
            }
            var bytes = _fileService.FromFile(request.file);
            if (request.file.ContentType == "image/heic"||request.file.ContentType== "image/heif")
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (MagickImage image = new MagickImage(bytes))
                    {
                        image.Format = MagickFormat.Jpg;
                        image.Write(memStream);
                    }
                    bytes = memStream.ToArray();
                }
            }
            var formData = new MultipartFormDataContent();
            var fileBytes = new ByteArrayContent(bytes);
            formData.Add(fileBytes, "file", request.file.FileName);
            formData.Add(new StringContent(_configuration.GetSection("Idx")["accessKey"]), "accessKey");
            formData.Add(new StringContent(_configuration.GetSection("Idx")["secretKey"]), "secretKey");
            var response = await _httpClient.PostAsync(_configuration.GetSection("Idx")["Urls:Passport"], formData);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic obj = JsonConvert.DeserializeObject(responseString);
            var fields = new Dictionary<string, string>();
            if (obj.resultCode == 0)
            {
                foreach (var property in obj.items[0].fields)
                {
                    var propertyPath = (string)property.Path;
                    var propertyName = propertyPath.Split(".")[2];
                    var propertyValue = (string)property.ToString();
                    var value = propertyValue.Split("\"")[5];
                    fields.Add(propertyName, value);
                }
            }
            var document = user.Requests.First().Documents.FirstOrDefault(x => x.DocumentType == Common.Entities.DocumentType.Passport);
            if (document != null)
            {
                var documentUrl = await _fileService.AddFile(request.file, user.Id, document.Id, "jpg");
                document.FileUrl = documentUrl;
                document.Created = DateTime.UtcNow;
                document.DocumentType = Common.Entities.DocumentType.Passport;
               await _historyService.AddHistory(user.Requests.First().Id, "Обновление паспорта");
            }
            if (document == null)
            {
                var newDocument = _context.Documents.Add(new Common.Entities.Document
                {
                    Created = DateTime.UtcNow,
                    RequestId = user.Requests.First().Id,
                    DocumentType = Common.Entities.DocumentType.Passport,
                });
                var documentUrl = await _fileService.AddFile(request.file, user.Id, newDocument.Entity.Id, "jpg");
                newDocument.Entity.FileUrl = documentUrl;
                await _historyService.AddHistory(user.Requests.First().Id, "Загрузка паспорта");

            }

            _context.SaveChanges();
            return new PassportUploadResponse
            {
                IsSuccess = true,
                Fields = fields.Count == 0 ? null : fields
            };
        }
    }
}
