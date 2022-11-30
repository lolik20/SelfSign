using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SelfSign.BL.Services;
using SelfSign.Common.Entities;
using SelfSign.Common.RequestModels;
using SelfSign.DAL;

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
        private readonly IMediator _mediator;
        public ITMonitoringController(ApplicationContext context, IConfiguration configuration, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
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
                _httpClient.DefaultRequestHeaders.Authorization=new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", responseString);
            _httpClient.Timeout = TimeSpan.FromSeconds(4);
        }
        
        [HttpGet("request")]
        public async Task<IActionResult> Request([FromQuery] CreateItMonitoringRequest request)
        {
            var response = await _mediator.Send(request);
            if (!response.IsSuccessful)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Message);
        }

        [HttpGet("twofactor")]
        public async Task<IActionResult> TwoFactor([FromQuery] ItMonitoringTwoFactorRequest request)
        {
            var response = await _mediator.Send(request);
            if (!response.IsSuccessful)
            {
                return BadRequest();
            }
            return Ok();
            
        }
        [HttpGet("documents/upload")]
        public async Task<IActionResult> SendDocuments([FromQuery] ItMonitoringPassportRequest request)
        {
            var response =await _mediator.Send(request);
            if (response.IsSuccessful)
            {
                return Ok(response.Message);
            }
            return BadRequest(response.Message);
        }


        private async Task<dynamic> Confirm(string requestId)
        {
            string url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.Confirmation).Value;
            var response = await _httpClient.PostAsync(url.Replace("$requestId", requestId), null);
            dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            return result;
        }
        [HttpGet("confirmation")]
        public async Task<IActionResult> Confirmation(Guid id)
        {
            var user = _context.Users.Include(x => x.Requests.OrderBy(x => x.Created)).FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var result = await Confirm(user.Requests.OrderBy(x => x.Created).First().RequestId);
            return Ok(result);
        }
        [HttpGet("blank")]
        public async Task<IActionResult> GetBlank([FromQuery] ItMonitoringBlankRequest request)
        {
            var response =await _mediator.Send(request);
            return Ok(response);
        }
        //[HttpGet("blank")]
        //public async Task<IActionResult> GetBlank([FromQuery] Guid id)
        //{

        //    var user = _context.Users.FirstOrDefault(x => x.Id == id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }
        //    var requestEntity = _context.Requests.OrderBy(x => x.Created).First(x => x.UserId == id);
        //    var document = requestEntity.Documents.OrderBy(x => x.Created).Where(x => x.DocumentType == DocumentType.Statement).FirstOrDefault();
        //    if (document != null)
        //    {
        //        return Ok();
        //    }
        //    var url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.GetRequest).Value;
        //    var response = await _httpClient.GetAsync(url.Replace("$requestId", user.Requests[0].RequestId));

        //    if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        var newFile = _context.Documents.Add(new Document
        //        {
        //            DocumentType = DocumentType.Statement,
        //            RequestId = requestEntity.Id
        //        });
        //        string fileUrl = "dd";
        //        //var fileUrl = await FileService.AddFile(await response.Content.ReadAsByteArrayAsync(), user.Id, newFile.Entity.Id,"pdf");
        //        newFile.Entity.FileUrl = fileUrl;
        //        _context.SaveChanges();
        //        return Ok();
        //    }
        //    return BadRequest(await response.Content.ReadAsStringAsync());
        //}
        private async Task<dynamic> Documents(string requestId)
        {
            string url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.Documents).Value;
            var response = await _httpClient.GetAsync(url.Replace("$requestId", requestId));
            dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            foreach (var document in result)
            {
                if (document.DocTypeCode == 6 && document.Files.Count == 0)
                {
                    bool isSended = await SendDocument(6, requestId);
                }
            }
            return result;
        }
        private async Task<bool> SendDocument(int docTypeCode, string requestId)
        {
            var document = _context.Documents.OrderBy(x => x.Created).FirstOrDefault(x => x.DocumentType == (DocumentType)docTypeCode);
            if (document == null)
            {
                return false;
            }
            var form = new MultipartFormDataContent();
            //var fileBytes = FileService.GetDocument(document.FileUrl);
            var fileBytes = new byte[0];
            var file = new ByteArrayContent(fileBytes);
            file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            form.Add(file, "file", "file.jpg");
            string url = urls.FirstOrDefault(x => x.Key == ITMonitoringMethods.File).Value;
            var response = await _httpClient.PostAsync(url.Replace("$docTypeCode", docTypeCode.ToString()).Replace("$requestId", requestId), form);
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
