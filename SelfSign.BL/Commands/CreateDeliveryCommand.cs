using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SelfSign.BL.Services;
using SelfSign.Common.RequestModels;
using SelfSign.Common.ResponseModels;
using SelfSign.DAL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.BL.Commands
{
    public class CreateDeliveryCommand : IRequestHandler<CreateDeliveryRequest, CreateDeliveryResponse>
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;
        private readonly System.Net.Http.Headers.MediaTypeHeaderValue _pdfMimeType;
        public CreateDeliveryCommand(ApplicationContext context, IConfiguration configuration,HttpClient httpClient)
        {
            _httpClient = httpClient;
            _context = context;
            _configuration = configuration;
            _pdfMimeType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        }

        public async Task<CreateDeliveryResponse> Handle(CreateDeliveryRequest request, CancellationToken cancellationToken)
        {
            var user = _context.Users.Include(x => x.Requests).ThenInclude(x => x.Documents).FirstOrDefault(x => x.Id == request.UserId);
            if (user == null)
            {
                return new CreateDeliveryResponse
                {
                    IsSuccess = false
                };
            }
            DateTime date = new DateTime();
            var isValidDate = DateTime.TryParseExact(request.DeliveryDate, "dd.MM.yyyy", CultureInfo.GetCultureInfo("ru-RU"), DateTimeStyles.None, out date);
            if (!isValidDate)
            {
                return new CreateDeliveryResponse
                {
                    IsSuccess = false
                };
            }
            bool isValidCladr = IsValidCladr(request.Kladr.ToString());
            if (!isValidCladr)
            {
                return new CreateDeliveryResponse { IsSuccess = false };
            }
            var requestEntity = user.Requests.OrderBy(x => x.Created).First();
            var newDeliveryEntity = _context.Deliveries.Add(new Common.Entities.Delivery
            {
                Cladr = request.Kladr,
                DeliveryDate = date.ToUniversalTime(),
                Created = DateTime.UtcNow,
                RequestId = requestEntity.Id,
                Time = request.Time,
                VerificationCenter = requestEntity.VerificationCenter
            });
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(request.Address), "address");
            formData.Add(new StringContent(request.Kladr.ToString()), "cladr");
            formData.Add(new StringContent(request.PhoneNumber), "phone");
            formData.Add(new StringContent(newDeliveryEntity.Entity.RequestId.ToString()), "id");
            formData.Add(new StringContent(request.DeliveryDate), "date");
            formData.Add(new StringContent(request.Time), "time");
            formData.Add(new StringContent($"{user.Surname} {user.Name} {user.Patronymic}"), "fio");

            var fileBytes = FileService.GetDocument(requestEntity.Documents.First(x => x.DocumentType == Common.Entities.DocumentType.Statement).FileUrl);
            var file = new ByteArrayContent(fileBytes);
            file.Headers.ContentType = _pdfMimeType;
            formData.Add(file, "file", "file.pdf");
            var response = await _httpClient.PostAsync(_configuration.GetSection("Delivery")["Urls:CreateDelivery"], formData);
            var responseString = await response.Content.ReadAsStringAsync();
            var requestString = await response.RequestMessage.Content.ReadAsStringAsync();

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return new CreateDeliveryResponse
                {
                    IsSuccess = false
                };
            }
            _context.SaveChanges();
            return new CreateDeliveryResponse()
            {
                IsSuccess = true
            };
        }
        public bool IsValidCladr(string cladr)
        {
            using (StreamReader stream = new StreamReader("cladr.json"))
            {
                string json = stream.ReadToEnd();
                List<JsonItem> items = JsonConvert.DeserializeObject<List<JsonItem>>(json);
                if (!items.Select(x => x.field_1).Contains(cladr))
                    return false;
            }
            return true;
        }
        private class JsonItem
        {
            public string field_1 { get; set; }
        }
    }

}
