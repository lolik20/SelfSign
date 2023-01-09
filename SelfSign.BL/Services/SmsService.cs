using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace SelfSign.BL.Services
{
    public static class SmsService
    {
        private static readonly HttpClient _httpClient;
        private static string _login = "mos-gis";
        private static string _password = "mT%23K1kW";
        static SmsService()
        {
            _httpClient = new HttpClient();
        }


        public static async Task<bool> SendSms(string phone, string message)
        {
            phone = phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace("+", "").Replace(" ", "");
            var url = $"https://auth.terasms.ru/outbox/send?login={_login}&password={_password}&target={phone}&sender=SignSelf&message={message}&type=sms";
            var response = await _httpClient.GetAsync(new Uri(url));
            return true;
        }
    }
}
