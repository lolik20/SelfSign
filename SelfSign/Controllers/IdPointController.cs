using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SelfSign.Common.Entities;
using SelfSign.Common.RequestModels;
using SelfSign.DAL;

namespace SelfSign.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdPointController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IMediator _mediator;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public IdPointController(ApplicationContext context, IMediator mediator, IConfiguration configuration)
        {
            _context = context;
            _mediator = mediator;
            _httpClient = new HttpClient();
            _configuration = configuration;
        }
        private async Task Authorize()
        {
            var credentials = _configuration.GetSection("IdPoint");
            var request = new
            {
                username = credentials.GetValue<string>("username"),
                password = credentials.GetValue<string>("password"),
            };
            var response = await _httpClient.PostAsync("https://trust-entry-rc.iitrust.ru/api/rb/auth", new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseObject = JsonConvert.DeserializeObject(responseString);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", responseObject.result.token);

        }
        [HttpGet]
        public async Task<IActionResult> Request(Guid id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var isRequested = await _mediator.Send(new IsRequestedRequest
            {
                Id = id,
                VerificationCenter = VerificationCenter.IdPoint
            });
            if (isRequested.IsRequested)
            {
                return BadRequest("Запрос уже есть");
            }

            var request = new
            {
                scenario = 80,
                callback = "http://127.0.0.1/callback",
                consumer = new
                {
                    last_name = user.Surname,
                    first_name = user.Name,
                    middle_name = user.Patronymic,
                    gender = user.Gender==Gender.Мужской?"М":"F",
                    birthed = user.BirthDate.ToString("yyyy-MM-dd"),
                    phone = user.Phone,
                    snils = user.Snils,
                    inn = user.Inn,
                    adresses = new
                    {
                        type = "permanent",
                        region = "",
                        index = "",
                        city = "",
                        street = "",
                        house = "",
                        apartment = "",
                    },
                    identities = new
                    {
                        type = "internal-passport",
                        series = user.Serial,
                        number = user.Number,
                        issued_by = user.SubDivisionAddress,
                        issued = user.IssueDate.ToString("yyyy-MM-dd")
                    }
                }
            };
            var response = await _httpClient.PostAsync("https://trust-entry-rc.iitrust.ru/api/rb/workflow", new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json"));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok();
            }
            return BadRequest();

        }
    }
}
