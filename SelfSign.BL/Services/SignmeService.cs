using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SelfSign.BL.Interfaces;
using SelfSign.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.BL.Services
{
    public class SignmeService : ISignmeService
    {
        private readonly HttpClient _httpClient;
        private readonly string _key;
        private readonly IConfigurationSection _urls;
        public SignmeService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _key = configuration.GetSection("SignMe")["Key"];
            _urls = configuration.GetSection("SignMe").GetSection("Urls");
        }
        public async Task<Tuple<bool, JObject, string?>> Create(User user, string shortCladr)
        {
            var signMeRequest = new
            {
                bdate = user.BirthDate.ToString("yyyy-MM-dd"),
                city = user.BirthPlace,
                country = "RU",
                email = user.Email,
                inn = user.Inn,
                issued = user.SubDivisionAddress,
                key = _key,
                lastname = user.Patronymic,
                name = user.Name,
                snils = user.Snils.Replace("-", "").Trim(),
                pcode = user.SubDivisionCode,
                pdate = user.IssueDate.ToString("yyyy-MM-dd"),
                phone = user.Phone,
                pn = user.Number,
                ps = user.Serial,
                surname = user.Surname,
                region = shortCladr,
                regtype = "1",
                gender = user.Gender == Gender.Мужской ? "M" : "F"
            };
            var requestJson = new StringContent(JsonConvert.SerializeObject(signMeRequest), Encoding.UTF8, "application/json");
            var signMeResponse = await _httpClient.PostAsync(_urls["Register"], requestJson);
            var signMeResponseString = await signMeResponse.Content.ReadAsStringAsync();

            try
            {
                JObject signMeResponseObject = JObject.Parse(signMeResponseString);
                string result = null;
                return Tuple.Create(true, signMeResponseObject, result);

            }
            catch
            {
                var result = new JObject();
                return Tuple.Create(false, result, signMeResponseString);
            }

        }
        public async Task<bool> UploadDocument(string snils, string base64, DocumentType documentType, string fileName, string fileExtension)
        {
            int doctype;
            string name="";
            switch (documentType)
            {
                case DocumentType.Statement:
                    doctype = 4;
                    name = "statement";
                    break;
                case DocumentType.Passport:
                    doctype = 1;
                    name = "passport";
                    break;
                case DocumentType.Snils:
                    doctype = 3;
                    name = "snils";
                    break;
                case DocumentType.PhotoWithStatement:
                    doctype = 7;
                    name = "statementPhoto";
                    break;
                default:
                    doctype = -1;
                    break;
            }
            var request = new
            {
                utype = 1,
                uid = snils,
                doctype = doctype,
                name = $"{name}.{fileExtension}",
                file = base64,
                key = _key
            };
            var requestString = JsonConvert.SerializeObject(request);
            var response = await _httpClient.PostAsync(_urls["UploadFile"], new StringContent(requestString,
                  Encoding.UTF8,
                  "application/json"));
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return true;
            }
            return false;
        }
        public async Task<PrecheckResponse> PreCheck(User user)
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(_key), "key");
            formData.Add(new StringContent(user.Phone), "phone");
            formData.Add(new StringContent(user.Email), "email");
            formData.Add(new StringContent(user.Snils), "snils");
            formData.Add(new StringContent(user.Inn), "inn");
            var isExistResponse = await _httpClient.PostAsync(_urls["PreCheck"], formData);
            var isExistResponseString = await isExistResponse.Content.ReadAsStringAsync();
            dynamic responseObj = JsonConvert.DeserializeObject(isExistResponseString);
            return new PrecheckResponse
            {
                Phone = responseObj?.phone?.created is null ? false : (bool)responseObj.phone.created,
                Email = responseObj?.email?.created is null ? false : (bool)responseObj.email.created,
                Inn = responseObj?.inn?.created is null ? false : (bool)responseObj.inn.created,
                Pdf = responseObj?.phone.pdf
            };

        }
    }
    public class PrecheckResponse
    {
        public bool Phone { get; set; }
        public bool Email { get; set; }
        public bool Inn { get; set; }
        public string? Pdf { get; set; }
    }
}
