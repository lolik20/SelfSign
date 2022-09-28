﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SelfSign.Entities;

namespace SelfSign.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ITMonitoringController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;
        private HttpClient _httpClient;
        private Dictionary<ITMonitoringMethods, string> urls;

        public ITMonitoringController(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
            urls = new Dictionary<ITMonitoringMethods, string>();
            urls.Add(ITMonitoringMethods.Authorize, "https://api-test.digitaldeal.pro/ds/v1/auth/token");
            urls.Add(ITMonitoringMethods.Request, "https://api-test.digitaldeal.pro/ds/v1/request");
            urls.Add(ITMonitoringMethods.TwoFactor, "https://api-test.digitaldeal.pro/ds/v1/requests/$requestId/dss/2fa");
            urls.Add(ITMonitoringMethods.Confirmation, "https://api-test.digitaldeal.pro/ds/v1/requests/$requestId/send");
            Authorize();
        }
        private async Task Authorize()
        {
            var credentials = _configuration.GetSection("ItMonitoring");
            var request = new
            {
                Login = credentials.GetValue<string>("Login"),
                Password = credentials.GetValue<string>("Password")
            };

            var response = await _httpClient.PostAsync(
                urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.Authorize).Value,
                new StringContent(JsonConvert.SerializeObject(request),
                System.Text.Encoding.UTF8,
                "application/json"));
            var requestStrin = await response.RequestMessage.Content.ReadAsStringAsync();
            var responseString = await response.Content.ReadAsStringAsync();
            _httpClient.DefaultRequestHeaders.Add("Authorization", responseString);
        }
        [HttpGet("request")]
        public async Task<IActionResult> Request([FromQuery]Guid id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var request = new
            {
                OwnerType=0,
                Contacts = new
                {
                    Email = user.Email,
                    Phone = user.Phone,
                },
                Address=new
                {
                    City = user.BirthPlace,
                    Value = user.RegAddress,
                },
                Owner = new
                {
                    Inn = user.Inn,
                    BirthDate=user.BirthDate.ToShortDateString(),
                    BirthPlace=user.BirthPlace,
                    Snils=user.Snils,
                    Passport = new
                    {
                        Series= user.Serial,
                        Number = user.Number,
                        IssueDate = user.RegDate.ToShortDateString(),
                        IssuingAuthorityCode =user.SubDivisionCode,
                        IssuingAuthorityName = user.SubDivisionAddress
                    },
                    FirstName = user.Name,
                    LastName =user.Surname,
                    MiddleName=user.Patronymic,
                    Gender=user.Gender,
                    CitizenshipCode=643
                },
                TarrifId= "3fa85f64-5717-4562-b3fc-2c963f66afa6"
            };
            string url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.Request).Value;
            var response = await _httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json"));
            dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            user.MyDssRequestId = result;

            return Ok(result);
        }
        [HttpGet("twofactor")]
        public async Task<IActionResult> TwoFactor([FromQuery]Guid id,string alias)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            string url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.TwoFactor).Value;
            var request = new
            {
                Dss2fa = 1,
                Codeword=alias
            };
            string requestId = "";
            var response = await _httpClient.PostAsync(url.Replace("$requestId",requestId ), new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json"));
            dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            return Ok(result);
        }
        [HttpPost("confirmation")]
        public async Task<IActionResult> Confirmation(Guid id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            string url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.Confirmation).Value;
            string requestId = "";
            var response = await _httpClient.PostAsync(url.Replace("$requestId", requestId),null);
            dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            return Ok(result);
        }

    }
    public enum ITMonitoringMethods
    {
        Authorize = 0,
        Request =1,
        TwoFactor =2,
        Confirmation=3
    }

}
