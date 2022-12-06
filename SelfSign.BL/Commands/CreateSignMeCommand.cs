using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SelfSign.Common.Entities;
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
    public class CreateSignMeCommand : IRequestHandler<SignMeRequest, SignMeResponse>
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _key;
        private readonly IConfigurationSection _urls;
        private readonly IMediator _mediator;
        public CreateSignMeCommand(ApplicationContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory, IMediator mediator)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient("SignMe");
            _key = _configuration.GetSection("SignMe")["Key"];
            _mediator = mediator;
            _urls=_configuration.GetSection("SignMe").GetSection("Urls");
        }

        public async Task<SignMeResponse> Handle(SignMeRequest request, CancellationToken cancellationToken)
        {

            var user = _context.Users.Include(x => x.Requests).FirstOrDefault(x => x.Id == request.Id);
            if (user == null || user.Requests.Count(x => x.VerificationCenter ==VerificationCenter.SignMe) > 0)
            {
                return new SignMeResponse
                {
                    IsSuccessful = false
                };
            }
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(_key), "key");
            formData.Add(new StringContent(user.Phone), "phone");
            formData.Add(new StringContent(user.Email), "email");
            formData.Add(new StringContent(user.Snils), "snils");
            formData.Add(new StringContent(user.Inn), "inn");
            var isExistResponse = await _httpClient.PostAsync(_urls["PreCheck"], formData);
            var isExistResponseString = await isExistResponse.Content.ReadAsStringAsync();
            dynamic responseObj = JsonConvert.DeserializeObject(isExistResponseString);
            if (responseObj?.snils?.requests[0] != null)
            {
                return new SignMeResponse { IsSuccessful = false, Message = "Завка уже существует" };
            }
            var cladr = await _mediator.Send(new AddressRequest { query = user.RegAddress });

            var signMeRequest = new
            {
                bdate = user.BirthDate.ToString("yyyy-MM-dd"),
                city = user.BirthPlace,
                country = "RU",
                email = user.Email,
                inn = user.Inn,
                issued = user.SubDivisionAddress,
                key = _key,
                lastname = user.Patronymic,
                name = user.Name,
                snils = user.Snils,
                pcode = user.SubDivisionCode,
                pdate = user.IssueDate.ToString("yyyy-MM-dd"),
                phone = user.Phone,
                pn = user.Number,
                ps = user.Serial,
                surname = user.Surname,
                region = cladr.First().ShortKladr.ToString(),
                regtype = "1",
                gender = user.Gender == Gender.Мужской ? "M" : "F"
            };
            var requestJson = new StringContent(JsonConvert.SerializeObject(signMeRequest), Encoding.UTF8, "application/json");
            var signMeResponse = await _httpClient.PostAsync(_urls["Register"], requestJson);

           
            var signMeResponseString = await signMeResponse.Content.ReadAsStringAsync();
            dynamic signMeResponseObject = JsonConvert.DeserializeObject<dynamic>(signMeResponseString);
            if ((string)signMeResponseObject.qr == null)
            {
                return new SignMeResponse
                {
                    IsSuccessful = false,
                    Message = (string)signMeResponseObject,
                };
            }
            _context.Requests.Add(new Request
            {
                Created = DateTime.Now.ToUniversalTime(),
                UserId = user.Id,
                RequestId = (string)signMeResponseObject.id,
                VerificationCenter = VerificationCenter.SignMe
            });
            _context.SaveChanges();
            return new SignMeResponse
            {
                IsSuccessful = true,
                Message = (string)signMeResponseObject.qr
            };
        }
    }
}
