using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SelfSign.Entities;
using System.Text;

namespace SelfSign.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdxController : ControllerBase
    {
        private HttpClient _httpClient;
        private IConfiguration _configuration;
        private Dictionary<IdxMethod, string> urls;
        private ApplicationContext _context;
        public IdxController(IConfiguration configuration, ApplicationContext context)
        {
            Initial(configuration);
            _context = context;
        }
        private void Initial(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _configuration = configuration;
            urls = new Dictionary<IdxMethod, string>();
            urls.Add(IdxMethod.First, "https://api.id-x.org/idx/api2/parseAuto/multiple/passport");
            urls.Add(IdxMethod.Second, "https://api.id-x.org/idx/api2/parseAuto/multiple/passportRegistration");
            urls.Add(IdxMethod.Inn, "https://service.nalog.ru/inn-proc.do");
            urls.Add(IdxMethod.Snils, "https://api.id-x.org/idx/api2/parseAuto/multiple/snils");

        }

        private void AddResponseHeaders()
        {
            var header = HttpContext.Response.Headers.FirstOrDefault(x => x.Key == "Access-Control-Allow-Origin");
            if (header.Key == null)
            {

                HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "X-Requested-With");
            }
        }
        private bool CheckUser(Guid id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return false;
            }
            return true;
        }
        private async Task AddDocument(IFormFile file, Guid userId, DocumentType documentType)
        {
            var newDocument = new Document
            {
                DocumentType = documentType,
                UserId = userId
            };
            var addedDocument = _context.Documents.Add(newDocument);
            string fileUrl = await FileService.AddFile(file, userId, addedDocument.Entity.Id);
            addedDocument.Entity.FileUrl = fileUrl;
            _context.SaveChanges();
        }
        [HttpPost("first")]
        public async Task<IActionResult> FirstPhoto([FromForm] PassportRequest request)
        {
            var isUser = CheckUser(request.Id);
            if (!isUser)
            {
                return NotFound();
            }
            var keys = _configuration.GetSection("Idx").AsEnumerable();
            var response = await PostData(request.file, keys, IdxMethod.First);
            if (response == null)
            {
                return BadRequest();
            }
            await AddDocument(request.file, request.Id, DocumentType.Passport);
            return Ok();
        }

        [HttpPost("second")]
        public async Task<IActionResult> SecondPhoto([FromForm] PassportRequest request)
        {
            var isUser = CheckUser(request.Id);
            if (!isUser)
            {
                return NotFound();
            }
            //var keys = _configuration.GetSection("Idx").AsEnumerable();
            //var response = await PostData(request.file, keys, IdxMethod.Second);
            //if(response == null)
            //{
            //    return BadRequest();
            //}
            await AddDocument(request.file, request.Id, DocumentType.Passport);

            return Ok();
        }
        [HttpPost("snils")]
        public async Task<IActionResult> Snils([FromForm] PassportRequest request)
        {
            var isUser = CheckUser(request.Id);
            if (!isUser)
            {
                return NotFound();
            }
            var keys = _configuration.GetSection("Idx").AsEnumerable();
            var response = await PostData(request.file, keys, IdxMethod.Snils);
            if (response == null)
            {
                return BadRequest();
            }
            await AddDocument(request.file, request.Id, DocumentType.Snils);

            return Ok(response);
        }
        [HttpPost("inn")]
        public async Task<IActionResult> Inn([FromBody] InnRequest request)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == request.Id);
            if (user == null)
            {
                return NotFound();
            }
            AddResponseHeaders();
            var keys = _configuration.GetSection("Idx");

            string url = urls.First(x => x.Key == IdxMethod.Inn).Value;
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(user.Surname), "fam");
            formData.Add(new StringContent(user.Name), "nam");
            formData.Add(new StringContent(user.Patronymic), "otch");
            formData.Add(new StringContent(user.BirthDate.ToString("dd.MM.yyyy")), "bdate");
            formData.Add(new StringContent(""), "bplace");
            formData.Add(new StringContent("21"), "doctype");
            formData.Add(new StringContent($"{user.Serial} {user.Number}"), "docno");
            formData.Add(new StringContent(user.IssueDate.ToString("dd.MM.yyyy")), "docdt");
            formData.Add(new StringContent("innMy"), "c");
            formData.Add(new StringContent(""), "captcha");
            formData.Add(new StringContent(""), "captchaToken");

            var response = await _httpClient.PostAsync(url, formData);
            var responseString = await response.Content.ReadAsStringAsync();
            var requestString = await response.RequestMessage.Content.ReadAsStringAsync();
            dynamic obj = JsonConvert.DeserializeObject(responseString);
            if (obj.code == 1)
            {
                return Ok((string)obj.inn);
            }
            return BadRequest();

        }

        private async Task<Dictionary<string,string>> PostData(IFormFile file, IEnumerable<KeyValuePair<string, string>> keys, IdxMethod method)
        {
            AddResponseHeaders();

            var form = new MultipartFormDataContent();
            var fileBytes = FromFile(file);
            form.Add(fileBytes, "file", file.FileName);
            foreach (var key in keys.Skip(1))
            {
                form.Add(new StringContent(key.Value), String.Format("\"{0}\"", key.Key.Split(":")[1]));
            }
            string url = urls.First(x => x.Key == method).Value;
            var response = await _httpClient.PostAsync(url, form);
            var responseString = await response.Content.ReadAsStringAsync();
            
            dynamic obj = JsonConvert.DeserializeObject(responseString);
            if (obj.resultCode == 0)
            {
                var result = new Dictionary<string, string>();
                foreach (var property in obj.items[0].fields)
                {
                    var propertyPath =(string) property.Path;
                    var propertyName = propertyPath.Split(".")[2];
                    var propertyValue = (string)property.ToString();
                    var value = propertyValue.Split("\"")[5];
                    result.Add(propertyName, value);
                }
                return result;
            }
            return null;
        }

        private ByteArrayContent FromFile(IFormFile formFile)
        {
            long length = formFile.Length;
            if (length < 0)
                return null;
            using var fileStream = formFile.OpenReadStream();
            byte[] bytes = new byte[length];
            fileStream.Read(bytes, 0, (int)formFile.Length);
            return new ByteArrayContent(bytes);
        }
    }
    public class PassportRequest
    {
        public Guid Id { get; set; }
        public IFormFile? file { get; set; }
    }
  
    public class InnRequest
    {
        public Guid Id { get; set; }

    }
    public 

    enum IdxMethod
    {
        First = 0,
        Second = 1,
        Inn = 2,
        Snils = 3,
    }
}
