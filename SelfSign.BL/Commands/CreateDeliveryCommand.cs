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
        private readonly IFileService _fileService;
        private readonly IMediator _mediator;
        private readonly IHistoryService _historyService;
        public CreateDeliveryCommand(ApplicationContext context, IConfiguration configuration, HttpClient httpClient, IFileService fileService, IMediator mediator, IHistoryService historyService)
        {
            _httpClient = httpClient;
            _context = context;
            _configuration = configuration;
            _pdfMimeType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            _fileService = fileService;
            _mediator = mediator;
            _historyService = historyService;
        }

        public async Task<CreateDeliveryResponse> Handle(CreateDeliveryRequest request, CancellationToken cancellationToken)
        {
            var user = _context.Users.Include(x => x.Requests.OrderByDescending(x => x.Created)).ThenInclude(x => x.Documents).FirstOrDefault(x => x.Id == request.UserId);
            if (user == null || user.Requests.Count() == 0)
            {
                return new CreateDeliveryResponse
                {
                    IsSuccess = false,
                    Message = "Пользователь не найден или нет заявок"
                };
            }
            DateTime date = new DateTime();
            var isValidDate = DateTime.TryParseExact(request.DeliveryDate, "dd.MM.yyyy", CultureInfo.GetCultureInfo("ru-RU"), DateTimeStyles.None, out date);
            if (!isValidDate || date < DateTime.Now)
            {
                return new CreateDeliveryResponse
                {
                    IsSuccess = false,
                    Message = "Неверная дата или доставка в прошлое не в наших силах"
                };
            }
            var cladrResponse = await _mediator.Send(new AddressRequest { query = request.Address });
            var cladr = cladrResponse.First().Kladr.ToString();
            bool isValidCladr = IsValidCladr(cladr);
            if (!isValidCladr)
            {
                return new CreateDeliveryResponse
                {
                    IsSuccess = false,
                    Message = "Доставка по данному адресу невозможна"
                };
            }
            var requestEntity = user.Requests.OrderBy(x => x.Created).First();
            var newDeliveryEntity = _context.Deliveries.Add(new Common.Entities.Delivery
            {
                Cladr = cladr,
                DeliveryDate = date.ToUniversalTime(),
                Created = DateTime.UtcNow,
                RequestId = requestEntity.Id,
                Time = request.Time,
                Address = request.Address,
                VerificationCenter = requestEntity.VerificationCenter,
                TrackNumber = new Random().Next(0, 1000000),
                PhoneNumber=request.PhoneNumber
            });
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(request.Address), "address");
            formData.Add(new StringContent(cladr), "cladr");
            formData.Add(new StringContent(request.PhoneNumber), "phone");
            formData.Add(new StringContent(newDeliveryEntity.Entity.Id.ToString()), "id");
            formData.Add(new StringContent(request.DeliveryDate), "date");
            formData.Add(new StringContent(request.Time), "time");
            formData.Add(new StringContent($"{user.Surname} {user.Name} {user.Patronymic}"), "fio");

            var fileBytes = _fileService.GetDocument(requestEntity.Documents.First(x => x.DocumentType == Common.Entities.DocumentType.Statement).FileUrl);
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
                    IsSuccess = false,
                    Message = "Ошибка при запросе доставки"
                };
            }
            await _historyService.AddHistory(requestEntity.Id, "Создание заявки на доставку");
            _context.SaveChanges();
            await SmsService.SendSms(request.PhoneNumber, $"Курьер привезет конверт с документами на подпись {request.DeliveryDate} во временном интервале {request.Time} по адресу: {request.Address}. Ваша ссылка для отслеживания статуса доставки https://signself.ru/trackNumber/{newDeliveryEntity.Entity.TrackNumber}");
            return new CreateDeliveryResponse()
            {
                IsSuccess = true,
                Message = newDeliveryEntity.Entity.TrackNumber.ToString()
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
