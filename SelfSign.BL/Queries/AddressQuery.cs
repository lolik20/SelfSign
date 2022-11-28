using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SelfSign.Common.RequestModels;
using SelfSign.Common.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.BL.Queries
{
    public class AddressQuery : IRequestHandler<AddressRequest, List<AddressResponse>>
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public AddressQuery(IConfiguration configuration,IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient("Dadata");
        }
        public async Task<List<AddressResponse>> Handle(AddressRequest request, CancellationToken cancellationToken)
        {
            var dadataSection = _configuration.GetSection("Dadata");
      
            var url = dadataSection["Urls:Address"];
            var response = await _httpClient.PostAsync($"{dadataSection["Urls:Address"]}", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();

            if ((int)response.StatusCode == 200)
            {
                var responseJson = JsonConvert.DeserializeObject<DadataWrapper>(responseString);
                return responseJson.suggestions.Select(x => new AddressResponse
                {
                    Value = x.value,
                    ShortKladr = x.data.region_kladr_id / 100000000000,
                    Kladr = x.data.region_kladr_id
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
            public long Kladr { get; set; }
            public long FullKladr { get; set; }
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
