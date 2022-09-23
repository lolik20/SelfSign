using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

        public IdxController(IConfiguration configuration)
        {
            Initial(configuration);
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
        [HttpPost("first")]
        public async Task<IActionResult> FirstPhoto([FromForm] PassportRequest request)
        {
            var keys = _configuration.GetSection("Idx").AsEnumerable();
            var response =await PostData(request.file, keys,IdxMethod.First);
            return Ok(response);
        }

        [HttpPost("second")]
        public async Task<IActionResult> SecondPhoto([FromForm] PassportRequest request)
        {
            var keys = _configuration.GetSection("Idx").AsEnumerable();
            var response = await PostData(request.file, keys, IdxMethod.First);
            return Ok(response);
        }
        [HttpPost("snils")]
        public async Task<IActionResult> Snils([FromForm] PassportRequest request)
        {
            var keys = _configuration.GetSection("Idx").AsEnumerable();
            var response = await PostData(request.file, keys, IdxMethod.Snils);
            return Ok(response);
        }
        [HttpPost("inn")]
        public async Task<IActionResult> Inn([FromBody] InnRequest request)
        {
            AddResponseHeaders();
            var keys = _configuration.GetSection("Idx");


            request.secretKey = keys.GetValue<string>("secretKey").Replace("\"","");
            request.accessKey = keys.GetValue<string>("accessKey").Replace("\"","");
            string url = urls.First(x => x.Key == IdxMethod.Inn).Value;
            var response = await _httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(request),Encoding.UTF8,"application/json"));
            var responseString = await response.Content.ReadAsStringAsync();
            var requestString = await response.RequestMessage.Content.ReadAsStringAsync();
            dynamic obj = JsonConvert.DeserializeObject(responseString);
            return Ok(obj);

        }
       
    private async Task<dynamic> PostData(IFormFile file, IEnumerable<KeyValuePair<string, string>> keys,IdxMethod method)
        {
            AddResponseHeaders();
            
            var form = new MultipartFormDataContent();
            var fileBytes = FromFile(file);
            form.Add(fileBytes,"file",file.FileName);
            foreach (var key in keys.Skip(1))
            {
                form.Add(new StringContent(key.Value), String.Format("\"{0}\"", key.Key.Split(":")[1]));
            }
            
            string url = urls.First(x => x.Key == method).Value;
            var response = await _httpClient.PostAsync(url, form);
            var requestString =await response.RequestMessage.Content.ReadAsStringAsync();
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
            return new ByteArrayContent( bytes);
        }
    }
    public class PassportRequest
    {
        public IFormFile file { get; set; }
    }
    public class InnRequest { 
        public string lastName { get; set; }
        public string firstName { get; set; }
        public string midName { get; set; }
        public string birthDate { get; set; }
        public string passportNumber { get; set; }
        public string passportDate { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public string accessKey { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]

        public string secretKey { get; set; }
    }


    enum IdxMethod { 
        First = 0,
        Second = 1,
        Inn =2,
        Snils = 3,
    }
}
