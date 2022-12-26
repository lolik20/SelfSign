using Microsoft.Extensions.Configuration;

namespace SelfSign.BL.Services
{
    public static class SmsService
    {
        private static readonly HttpClient _httpClient;
        private static string _login = "mos-gis";
        private static string _password = "O~Zf86F";
        static SmsService()
        {
            _httpClient = new HttpClient();
        }
   

        public static async Task<bool> SendSms(string phone, string message)
        {
            var response = await _httpClient.GetAsync($"http://auth.terasms.ru/outbox/send?login={_login}&password={_password}&target={phone}&sender=terasms.ru&message={message}");
            return true;
        }
    }
}
