using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SelfSign.BL.Interfaces;
using SelfSign.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.BL.Services
{
    public class ItMonitoringService : IItMonitoringService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _urls;
        public ItMonitoringService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _urls = _configuration.GetSection("ItMonitoring").GetSection("Urls");
            Authorize().ConfigureAwait(true);
        }
        private async Task Authorize()
        {

            var itMonitoringCredentials = _configuration.GetSection("ItMonitoring");
            var request = new
            {
                Login = itMonitoringCredentials["Login"],
                Password = itMonitoringCredentials["Password"]
            };
            var authResponse = await _httpClient.PostAsync(_urls["Authorize"],
                  new StringContent(JsonConvert.SerializeObject(request),
                  Encoding.UTF8,
                  "application/json"));
            var responseString = await authResponse.Content.ReadAsStringAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", responseString);
        }
        public async Task<Tuple<bool, string>> CreateRequest(object request)
        {
        start:
            var response = await _httpClient.PostAsync(_urls["CreateRequest"], new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
            dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await Authorize();
                goto start;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return Tuple.Create(true, (string)result);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return Tuple.Create(false, (string)result.innerContent.message);
            }
            string errors = "";
            foreach (var error in result?.errors)
            {
                errors += $"{error};";
            }
            return Tuple.Create(false, errors);
        }
        public async Task<bool> TwoFactor(string requestId, object request)
        {
        start:
            var response = await _httpClient.PostAsync(_urls["TwoFactor"].Replace("$requestId", requestId), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await Authorize();
                goto start;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }
        public async Task<dynamic> GetDocuments(string requestId)
        {
            var response = await _httpClient.GetAsync(_urls["Documents"].Replace("$requestId", requestId));
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseString);
            foreach (var document in result)
            {

            }
            return result;
        }
        public async Task<byte[]?> GetDocument(string requestId, DocumentType documentType)
        {
            var response = await _httpClient.GetAsync(_urls["GetDocument"].Replace("$requestId", requestId).Replace("$docTypeCode", $"{(int)documentType}"));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await response.Content.ReadAsByteArrayAsync();
                return result;
            }
            return null;
        }
        public async Task<bool> UploadDocuments(string requestId, byte[] fileBytes, DocumentType documentType, string fileName, string fileExtension, string mimeType)
        {
            var form = new MultipartFormDataContent();
            var file = new ByteArrayContent(fileBytes);
            file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
            form.Add(file, "file", $"{fileName}.{fileExtension}");
            var response = await _httpClient.PostAsync(_urls["UploadFiles"].Replace("$docTypeCode", documentType.ToString()).Replace("$requestId", requestId), form);
            var responseString = await response.Content.ReadAsStringAsync();
            return true;
        }
        public async Task<bool> Confirmation(string requestId)
        {
            var response = await _httpClient.PostAsync(_urls["Confirmation"].Replace("$requestId", requestId), null);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }
        public async Task<bool> UploadDocuments(string requestId, Dictionary<byte[], DocumentType> documents)
        {
            //var form = new MultipartFormDataContent();
            //var file = new ByteArrayContent(fileBytes);
            //file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            //form.Add(file, "file", "file.jpg");
            //var response = await _httpClient.PostAsync(_urls["UploadFiles"].Replace("$docTypeCode", documentType.ToString()).Replace("$requestId", requestId), form);
            //var responseString = await response.Content.ReadAsStringAsync();
            return true;
        }
    }

}
