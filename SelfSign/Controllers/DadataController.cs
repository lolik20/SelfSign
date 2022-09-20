
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
            var requestModel = new
            {
                query = request
            };
            var response = await _httpClient.PostAsync("https://suggestions.dadata.ru/suggestions/api/4_1/rs/suggest/address", new StringContent(JsonConvert.SerializeObject(requestModel),System.Text.Encoding.UTF8,"application/json"));
            var responseString = await response.Content.ReadAsStringAsync();

            if ((int)response.StatusCode == 200)
            {
                dynamic responseJson = JsonConvert.DeserializeObject<DadataWrapper>( responseString);
                return Ok(responseJson.suggestions);
            }
            return BadRequest();
        }
    }
    public class DadataWrapper { 
        
        public List< DadataResponse> suggestions { get; set; }
    }
    public class DadataResponse
    {
        public string value { get; set; }
    }

}

