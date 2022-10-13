using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            urls.Add(ITMonitoringMethods.File, "https://api-test.digitaldeal.pro/ds/v1/requests/$requestId/documents/$docTypeCode/files");
            urls.Add(ITMonitoringMethods.GetRequest, "https://api-test.digitaldeal.pro/ds/v1/requests/$requestId/documents/1/template");


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
                    Gender = user.Gender,
                    CitizenshipCode = 643
                },
                TariffId = "deac4065-0433-497d-80b8-34784f261261"
            };
            string url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.Request).Value;
            var response = await _httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json"));

            var responseString = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseString);
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                user.MyDssRequestId = Guid.Parse(result);

                _context.SaveChanges();
                return Ok(result);
            }
            var errors = new List<string>();
            foreach (var error in result?.errors)
            {
                errors.Add(error.Path);
            }
            return BadRequest(errors);
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
                Dss2fa = 2,
                Codeword = alias
            };
            string url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.TwoFactor).Value;

            Guid requestId = (Guid)user.MyDssRequestId;
            var response = await _httpClient.PostAsync(url.Replace("$requestId", requestId.ToString()), new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json"));
            dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(result);

            }
            return BadRequest(result);
        }
        [HttpGet("documents/upload")]
        public async Task<IActionResult> SendDocuments([FromQuery] Guid id)
        {
            var user = _context.Users.Include(x=>x.Documents).FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var document = user.Documents.Where(x => x.DocumentType == DocumentType.Passport).FirstOrDefault();
            if (document != null)
            {
                return Ok();
            }
            var documents = await Documents(user);
            var result = await Confirm(user);
            return Ok(result);
        }
        private async Task<dynamic> Confirm(User user)
        {
            await Authorize();
            string url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.Confirmation).Value;
            Guid requestId = (Guid)user.MyDssRequestId;
            var response = await _httpClient.PostAsync(url.Replace("$requestId", requestId.ToString()), null);
            dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            return result;
        }
        [HttpGet("confirmation")]
        public async Task<IActionResult> Confirmation(Guid id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var result = await Confirm(user);
            return Ok(result);
        }
        [HttpGet("blank")]
        public async Task<IActionResult> GetBlank(Guid id)
        {
            await Authorize();

            var user = _context.Users.Include(x => x.Documents).FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var document = user.Documents.Where(x => x.DocumentType == DocumentType.Statement).FirstOrDefault();
            if (document != null)
            {
                return Ok();
            }
            var url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.GetRequest).Value;
            var response = await _httpClient.GetByteArrayAsync(url.Replace("$requestId", user.MyDssRequestId.ToString()));
            var newFile = _context.Documents.Add(new Document
            {
                DocumentType = DocumentType.Statement,
                UserId = user.Id
            }) ;
            var fileUrl =await FileService.AddFile(response, user.Id, newFile.Entity.Id);
            newFile.Entity.FileUrl = fileUrl;
            _context.SaveChanges();
            return Ok();
        }
        private async Task<dynamic> Documents(User user)
        {
            await Authorize();
            string url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.Documents).Value;
            Guid requestId = (Guid)user.MyDssRequestId;
            var response = await _httpClient.GetAsync(url.Replace("$requestId", requestId.ToString()));
            dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            foreach (var document in result)
            {
                if (document.DocTypeCode == 6)
                {
                    bool isSended = await SendDocument(6, requestId);

                }
            }
            return result;
        }
        private async Task<bool> SendDocument(int docTypeCode, Guid requestId)
        {
            var document = _context.Documents.FirstOrDefault(x => x.DocumentType == (DocumentType)docTypeCode);
            if (document == null)
            {
                return false;
            }
            var form = new MultipartFormDataContent();
            var fileBytes = FileService.GetDocument(document.FileUrl);
            var file = new ByteArrayContent(fileBytes);
            file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            form.Add(file, "file", "file.jpg");
            string url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.File).Value;
            var response = await _httpClient.PostAsync(url.Replace("$docTypeCode", docTypeCode.ToString()).Replace("$requestId", requestId.ToString()), form);
            var responseString = await response.Content.ReadAsStringAsync();
            var requestString = await response.RequestMessage.Content.ReadAsStringAsync();
            return true;
        }
     

    }
    public enum ITMonitoringMethods
    {
        Authorize = 0,
        Request = 1,
        TwoFactor = 2,
        Confirmation = 3,
        Documents = 4,
        File = 5,
        GetRequest
    }

}
