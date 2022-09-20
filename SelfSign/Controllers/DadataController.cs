
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace SelfSign.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DadataController:ControllerBase
    {
        private static readonly HttpClient _httpClient;
        private static readonly string ApiKey = "2e4092e70b939226cedb8cd26b558a10fe3d2fce";
        private static readonly string SecretKey = "c7c587fd62ef10de80ec97c6e00a9244e0fe20b9";
        static DadataController()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-Secret", SecretKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization",$"Token {ApiKey}");

        }
        [HttpGet("address")]
        public async Task<IActionResult> Address(string request) 
        {
            HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "X-Requested-With");

            var response = await _httpClient.PostAsync("https://cleaner.dadata.ru/api/v1/clean/address",new StringContent($"{System.Text.Json.JsonSerializer.Serialize(new[] {request})}",System.Text.Encoding.UTF8,"application/json"));
            if ((int)response.StatusCode == 200)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJson = JsonConvert.DeserializeObject<List<DadataResponse>>( responseString);
                return Ok(responseJson);
            }
            return BadRequest();
        }
    }
    public class DadataResponse
    {
        public string region_kladr_id { get; set; }
        public string country { get; set; }
        public string result { get; set; }
    }
}

