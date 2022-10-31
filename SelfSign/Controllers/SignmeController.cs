using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SelfSign.Common.Entities;
using SelfSign.Common.RequestModels;
using SelfSign.DAL;

namespace SelfSign.Controllers
{

    public class SignmeController : ApiControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;
        private readonly string Key;
        private readonly HttpClient _httpClient;
        public SignmeController(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            Key = _configuration.GetSection("SignMe").GetValue<string>("Key");
            _httpClient = new HttpClient();
        }
        [HttpGet("check")]
        public async Task<IActionResult> Check(Guid id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(Key), "key");
            formData.Add(new StringContent(user.Phone), "phone");
            formData.Add(new StringContent(user.Email), "email");
            formData.Add(new StringContent(user.Snils), "snils");
            formData.Add(new StringContent(user.Inn), "inn");
            var response = await _httpClient.PostAsync("https://test.sign.me/register/precheck", formData);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseObj = JsonConvert.DeserializeObject(responseString);
            if (responseObj?.snils?.requests[0] != null)
            {
                return BadRequest("Уже есть заявка в Sign Me");
            }
            return Ok();
        }
        [HttpGet("request")]
        public async Task<IActionResult> Request(Guid id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var isRequested = await Mediator.Send(new IsRequestedRequest
            {
                Id = id,
                VerificationCenter = VerificationCenter.ItMonitoring
            });
            if (isRequested.IsRequested)
            {
                return BadRequest("Запрос уже есть");
            }

            var requestData = new Request
            {
                bdate = user.BirthDate.ToString("yyyy-MM-dd"),
                city = user.BirthPlace,
                country = "RU",
                email = user.Email,
                inn = user.Inn,
                issued = user.SubDivisionAddress,
                key = Key,
                lastname = user.Patronymic,
                name = user.Name,
                snils = user.Snils,
                pcode = user.SubDivisionCode,
                pdate = user.IssueDate.ToString("yyyy-MM-dd"),
                phone = user.Phone,
                pn = user.Number,
                ps = user.Serial,
                surname = user.Surname,
                region = user.RegionCode.ToString(),
                regtype = "1",
                gender = user.Gender == Gender.Мужской ? "M" : "F"
            };

            var requestJson = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
            var requestResponse = await _httpClient.PostAsync("https://test.sign.me/register/api", requestJson);
            var requestResponseString = await requestResponse.Content.ReadAsStringAsync();
            dynamic requestResponseObj = JsonConvert.DeserializeObject(requestResponseString);
            if (requestResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return BadRequest("Ошибка");
            }
            _context.Requests.Add(new Common.Entities.Request
            {
                Created = DateTime.Now.ToUniversalTime(),
                UserId = id,
                RequestId=(string)requestResponseObj.id,
                VerificationCenter = VerificationCenter.SignMe
            });
            _context.SaveChanges();
            return Ok((string)requestResponseObj.qr);
        }

    }
    public class Request
    {
        public string key { get; set; }
        public string surname { get; set; }
        public string name { get; set; }
        public string lastname { get; set; }
        public string bdate { get; set; }
        public string pdate { get; set; }
        public string ps { get; set; }
        public string pn { get; set; }
        public string issued { get; set; }
        public string pcode { get; set; }
        public string country { get; set; }
        public string region { get; set; }
        public string city { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string snils { get; set; }
        public string inn { get; set; }
        public string regtype { get; set; }
        public string gender { get; set; }
    }
}
