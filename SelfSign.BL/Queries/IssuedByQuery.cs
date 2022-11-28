using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SelfSign.Common.RequestModels;
using SelfSign.Common.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.BL.Queries
{
    public class IssuedByQuery : IRequestHandler<IssuedByRequest, List<IssuedByResponse>>
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public IssuedByQuery(IConfiguration configuration,IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient("Dadata");
        }
        public async Task<List<IssuedByResponse>> Handle(IssuedByRequest request, CancellationToken cancellationToken)
        {
            var dadataSection = _configuration.GetSection("Dadata");
            var url = dadataSection["Urls:Address"];
            var response = await _httpClient.PostAsync($"{dadataSection["Urls:Issued"]}", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();

            if ((int)response.StatusCode == 200)
            {
                var responseJson = JsonConvert.DeserializeObject<DadataWrapper>(responseString);
                return responseJson.suggestions.Select(x => new IssuedByResponse
                {
                    Value = x.value
                }).ToList();
            }
            return null;
        }
        private class DadataWrapper
        {
            public List<DadataResponse> suggestions { get; set; }
        }
        private class FormatedObject
        {
            public string Value { get; set; }
        }

        private class DadataResponse
        {
            public string value { get; set; }
            public Data data { get; set; }
        }
        private class Data
        {
            public long? region_kladr_id { get; set; }
        }
    }
}
