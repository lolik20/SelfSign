
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace SelfSign.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DadataController : ControllerBase
    {
        private HttpClient _httpClient;
        private IConfiguration _configuration;
        private Dictionary<DadataMethod, string> urls;
        

        public DadataController(IConfiguration configuration)
        {
            Initial(configuration);
        }
        private void Initial(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            urls = new Dictionary<DadataMethod, string>();
            var dadataSection = _configuration.GetSection("Dadata");
            _httpClient.DefaultRequestHeaders.Add("X-Secret", dadataSection.GetValue<string>("Secret"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {dadataSection.GetValue<string>("Api")}");
            urls.Add(DadataMethod.Address, "https://suggestions.dadata.ru/suggestions/api/4_1/rs/suggest/address");
            urls.Add(DadataMethod.Fms, "https://suggestions.dadata.ru/suggestions/api/4_1/rs/suggest/fms_unit");
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
        private async Task<List<DadataResponse>> GetData(string request, DadataMethod method)
        {
            AddResponseHeaders();
            var requestModel = new
            {
                query = request
            };
            string url = urls.First(x => x.Key == method).Value;
            var response = await _httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(requestModel), System.Text.Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();

          if ((int)response.StatusCode == 200)
            {
                var responseJson = JsonConvert.DeserializeObject<DadataWrapper>(responseString);
                return responseJson.suggestions;
            }
            return null;
        }
        [HttpGet("address")]
        public async Task<IActionResult> Address(string request)
        {
            var response = await GetData(request, DadataMethod.Address);
            if (response != null)
            {
                return Ok(response);
            }
            return BadRequest();
        }
        [HttpGet("issuedby")]
        public async Task<IActionResult> IssuedBy(string request)
        {
            var response =await GetData(request, DadataMethod.Fms);
            if (response != null)
            {
                return Ok(response);
            }
            return BadRequest();
        }
    }
    public class DadataWrapper
    {

        public List<DadataResponse> suggestions { get; set; }
    }
    public class DadataResponse
    {
        public string value { get; set; }
        public Data data { get; set; } 
    }
    public class Data
    {
        public long city_kladr_id { get; set; }
    }
    enum DadataMethod
    {
        Address = 0,
        Fms = 1
    }


}

