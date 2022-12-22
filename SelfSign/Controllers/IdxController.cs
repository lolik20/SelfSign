using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SelfSign.BL.Services;
using SelfSign.Common.Entities;
using SelfSign.Common.RequestModels;
using SelfSign.DAL;
using System.Text;

namespace SelfSign.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdxController : ControllerBase
    {
        private HttpClient _httpClient;
        private IConfiguration _configuration;
        private Dictionary<IdxMethod, string> urls;
        private ApplicationContext _context;
        private readonly IMediator _mediator;
        public IdxController(IConfiguration configuration, ApplicationContext context,IMediator mediator)
        {
            Initial(configuration);
            _context = context;
            _mediator = mediator;
        }
        private void Initial(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _configuration = configuration;
            urls = new Dictionary<IdxMethod, string>();

            urls.Add(IdxMethod.Inn, "https://service.nalog.ru/inn-proc.do");
          
        }
      
        [HttpPost("first")]
        public async Task<IActionResult> FirstPhoto([FromForm] PassportUploadRequest request)
        {

            var response =await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPost("snils")]
        public async Task<IActionResult> Snils([FromForm] SnilsUpdateRequest request)
        {
            var response =await _mediator.Send(request);
            return Ok(response);
        }
        [HttpPost("inn")]
        public async Task<IActionResult> Inn([FromBody] InnRequest request)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == request.Id);
            if (user == null)
            {
                return NotFound();
            }
            var keys = _configuration.GetSection("Idx");

            string url = urls.First(x => x.Key == IdxMethod.Inn).Value;
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(user.Surname), "fam");
            formData.Add(new StringContent(user.Name), "nam");
            formData.Add(new StringContent(user.Patronymic), "otch");
            formData.Add(new StringContent(user.BirthDate.ToString("dd.MM.yyyy")), "bdate");
            formData.Add(new StringContent(""), "bplace");
            formData.Add(new StringContent("21"), "doctype");
            formData.Add(new StringContent($"{user.Serial} {user.Number}"), "docno");
            formData.Add(new StringContent(user.IssueDate.ToString("dd.MM.yyyy")), "docdt");
            formData.Add(new StringContent("innMy"), "c");
            formData.Add(new StringContent(""), "captcha");
            formData.Add(new StringContent(""), "captchaToken");
            var response = await _httpClient.PostAsync(url, formData);
            var responseString = await response.Content.ReadAsStringAsync();
            var requestString = await response.RequestMessage.Content.ReadAsStringAsync();
            dynamic obj = JsonConvert.DeserializeObject(responseString);
            if (obj.code == 1)
            {
                return Ok((string)obj.inn);
            }
            return BadRequest();

        }

       

       
    }
   
  
    public class InnRequest
    {
        public Guid Id { get; set; }

    }
    public 

    enum IdxMethod
    {
        First = 0,
        Second = 1,
        Inn = 2,
        Snils = 3,
    }
}
