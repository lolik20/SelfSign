
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace SelfSign.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DadataController : ControllerBase
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private static readonly string ApiKey = "2e4092e70b939226cedb8cd26b558a10fe3d2fce";
        private static readonly string SecretKey = "c7c587fd62ef10de80ec97c6e00a9244e0fe20b9";
        private static readonly Dictionary<Method, string> urls = new Dictionary<Method, string>();

        static DadataController()
        {
            _httpClient.DefaultRequestHeaders.Add("X-Secret", SecretKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {ApiKey}");
            urls.Add(Method.Address, "https://suggestions.dadata.ru/suggestions/api/4_1/rs/suggest/address");
            urls.Add(Method.Fms, "https://suggestions.dadata.ru/suggestions/api/4_1/rs/suggest/fms_unit");
            
        }
        private void AddResponseHeaders()
        {
            var header = HttpContext.Response.Headers.FirstOrDefault(x => x.Key == "Access-Control-Allow-Origin");
            if (header.Key==null) {

                HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "X-Requested-With");
            }
        }
        private async Task<List<DadataResponse>> GetData(string request, Method method)
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
            var response =await GetData(request, Method.Address);
            if (response != null)
            {
                return Ok(response);
            }
            return BadRequest();
        }
        [HttpGet("issuedby")]
        public async Task<IActionResult> IssuedBy(string request)
        {
            var response = GetData(request, Method.Fms);
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
    }
    public enum Method
    {
        Address = 0,
        Fms = 1
    }


}

