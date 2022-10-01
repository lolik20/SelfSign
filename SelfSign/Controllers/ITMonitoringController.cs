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
            urls.Add(ITMonitoringMethods.Request, "https://api-test.digitaldeal.pro/ds/v1/requests");
            urls.Add(ITMonitoringMethods.TwoFactor, "https://api-test.digitaldeal.pro/ds/v1/requests/$requestId/dss/2fa");
            urls.Add(ITMonitoringMethods.Confirmation, "https://api-test.digitaldeal.pro/ds/v1/requests/$requestId/send");
            urls.Add(ITMonitoringMethods.Documents, "https://api-test.digitaldeal.pro/ds/v1/requests/$requestId/documents");
        }
        private async Task Authorize()
        {
            var authToken = _httpClient.DefaultRequestHeaders.FirstOrDefault(x => x.Key == "Authorization");
            if (authToken.Value == null)
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
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {responseString}");
            }
            
        }
        [HttpGet("request")]
        public async Task<IActionResult> Request([FromQuery] Guid id)
        {
            
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            await Authorize();
            var gender = new Gender();
            Enum.TryParse(user.Gender, out gender);
            var request = new
            {
                OwnerType = 1,
                Contacts = new
                {
                    Email = user.Email,
                    Phone = user.Phone,
                },
                Address = new
                {
                    City = user.BirthPlace,
                    Value = user.RegAddress,
                    RegionCode = user.RegionCode
                },
                Owner = new
                {
                    Inn = user.Inn,
                    BirthDate = user.BirthDate.ToString("yyyy-MM-dd"),
                    BirthPlace = user.BirthPlace,
                    Snils = user.Snils,
                    Passport = new
                    {
                        Series = user.Serial,
                        Number = user.Number,
                        IssueDate = user.RegDate.ToString("yyyy-MM-dd"),
                        IssuingAuthorityCode = user.SubDivisionCode,
                        IssuingAuthorityName = user.SubDivisionAddress
                    },
                    FirstName = user.Name,
                    LastName = user.Surname,
                    MiddleName = user.Patronymic,
                    Gender = (int)gender,
                    CitizenshipCode = 643
                },
                TariffId = "8d9ea681-3cdb-4aa5-9ba6-7cb91f5e120a"
            };
            string url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.Request).Value;
            var response = await _httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json"));
            
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseString);
            if ((int)result.status != 400)
            {
                user.MyDssRequestId = result;

                _context.SaveChanges();
                return Ok(result);
            }
            return BadRequest(result);
        }
        [HttpGet("twofactor")]
        public async Task<IActionResult> TwoFactor([FromQuery] Guid id, string alias)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            await Authorize();
            var request = new
            {
                Dss2fa = 1,
                Codeword = alias
            };
            string url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.TwoFactor).Value;

            Guid requestId = (Guid)user.MyDssRequestId;
            var response = await _httpClient.PostAsync(url.Replace("$requestId", requestId.ToString()), new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json"));
            dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            return Ok(result);
        }
        [HttpGet("confirmation")]
        public async Task<IActionResult> Confirmation(Guid id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            await Authorize();
            string url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.Confirmation).Value;
            Guid requestId = (Guid)user.MyDssRequestId;
            var response = await _httpClient.PostAsync(url.Replace("$requestId", requestId.ToString()), null);
            dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            return Ok(result);
        }
        [HttpGet("documents")]
        public async Task<IActionResult> GetDocuments(Guid id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            await Authorize();
            string url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.Documents).Value;
            Guid requestId = (Guid)user.MyDssRequestId; 
            var response = await _httpClient.PostAsync(url.Replace("$requestId", requestId.ToString()), null);
            dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            return Ok(result);
        }

    }
    public enum ITMonitoringMethods
    {
        Authorize = 0,
        Request = 1,
        TwoFactor = 2,
        Confirmation = 3,
        Documents=4
    }

}
