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
            urls.Add(IdxMethod.Inn, "https://api.id-x.org/idx/api2/getInn");
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
            await AddDocument(request.file, request.Id, DocumentType.Passport);
            return Ok(null);
        }

        [HttpPost("second")]
        public async Task<IActionResult> SecondPhoto([FromForm] PassportRequest request)
        {
            var isUser = CheckUser(request.Id);
            if (!isUser)
            {
                return NotFound();
            }
            var keys = _configuration.GetSection("Idx").AsEnumerable();
            var response = await PostData(request.file, keys, IdxMethod.First);
            await AddDocument(request.file, request.Id, DocumentType.Passport);

            return Ok(null);
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
            var idxRequest = new InnIdxRequest
            {
                birthDate = user.BirthDate.ToString("yyyy-MM-dd"),
                passportDate = user.IssueDate.ToString("yyyy-MM-dd"),
                accessKey = keys.GetValue<string>("secretKey"),
                secretKey = keys.GetValue<string>("accessKey"),
                firstName = user.Name,
                lastName = user.Surname,
                midName = user.Patronymic,
                passportNumber = user.Serial + user.Number
            };
            var response = await _httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();
            var requestString = await response.RequestMessage.Content.ReadAsStringAsync();
            dynamic obj = JsonConvert.DeserializeObject(responseString);
            return Ok(obj);

        }

        private async Task<dynamic> PostData(IFormFile file, IEnumerable<KeyValuePair<string, string>> keys, IdxMethod method)
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
            var requestString = await response.RequestMessage.Content.ReadAsStringAsync();
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic obj = JsonConvert.DeserializeObject(responseString);
            return obj;
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
    public class InnIdxRequest
    {
        public string lastName { get; set; }
        public string firstName { get; set; }
        public string midName { get; set; }
        public string birthDate { get; set; }
        public string passportNumber { get; set; }
        public string passportDate { get; set; }
        public string accessKey { get; set; }
        public string secretKey { get; set; }
    }
    public class InnRequest
    {
        public Guid Id { get; set; }

    }


    enum IdxMethod
    {
        First = 0,
        Second = 1,
        Inn = 2,
        Snils = 3,
    }
}
