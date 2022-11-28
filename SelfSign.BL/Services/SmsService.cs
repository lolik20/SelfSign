namespace SelfSign.BL.Services
{
    public static class SmsService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static string _login = "Selfsign";
        private static string _password = "self456";

        public static async Task<bool> SendSms(string phone, string message)
        {
            var response = await _httpClient.GetAsync($"http://cab.websms.ru/http_in6.asp?http_username={_login}&http_password={_password}&Phone_list={phone}&Message={message}");
            //var responseString = await response.Content.ReadAsStringAsync();
            return true;
        }
    }
}
