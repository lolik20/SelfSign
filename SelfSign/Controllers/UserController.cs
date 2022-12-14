using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelfSign.BL.Interfaces;
using SelfSign.BL.Services;
using SelfSign.Common.Entities;
using SelfSign.Common.RequestModels;
using SelfSign.DAL;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SelfSign.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _regex;
        private readonly IMediator _mediator;
        private readonly ISignmeService _signmeService;
        private readonly IFileService _fileService;
        public UserController(ApplicationContext context, IConfiguration configuration, IMediator mediator, ISignmeService signmeService, IFileService fileService, IEncryptionService encryptionService)
        {
            _context = context;
            _configuration = configuration;
            _regex = _configuration.GetSection("regex");
            _mediator = mediator;
            _fileService = fileService;
            _signmeService = signmeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUser([FromQuery] Guid id)
        {
            var user = _context.Users.Include(x => x.Requests.OrderByDescending(x => x.Created)).FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(new
            {
                Name = user.Name,
                Surname = user.Surname,
                Patronymic = user.Patronymic,
                Center = user.Requests.First().VerificationCenter.ToString().ToLower(),
                Id = user.Id,
                Email = user.Email,
                Phone = user.Phone,
                BirthDate = user.BirthDate.ToString("dd.MM.yyyy"),
                RegDate = user.RegDate.ToString("dd.MM.yyyy"),
                IssueDate = user.IssueDate.ToString("dd.MM.yyyy"),
                Serial = user.Serial,
                SubDivisionAddress = user.SubDivisionAddress,
                SubDivisionCode = user.SubDivisionCode,
                BirthPlace = user.BirthPlace,
                Gender = user.Gender,
                Inn = user.Inn,
                Number = user.Number,
                RegAddress = user.RegAddress,
                Snils = user.Snils,
                Citizenship = user.Citizenship,
            });
        }
        [HttpGet("test")]
        public async Task<IActionResult> test([FromQuery] Guid id)
        {
            var user = _context.Users.Include(x => x.Requests).ThenInclude(x => x.Documents).FirstOrDefault(x => x.Id == id);
            var requestEntity = user.Requests.First();
            var document = requestEntity.Documents.First(x => x.DocumentType == DocumentType.Statement);
            var isDocument = await _signmeService.UploadDocument(user.Snils.Replace("-", ""), _fileService.GetBase64(document.FileUrl), document.DocumentType, document.Id.ToString(), "pdf");
            if (isDocument)
            {
                return Ok();
            }
            return BadRequest();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            //var validPhone = Regex.Match(request.Phone, _regex.GetValue<string>("Phone"));
            //if (!validPhone.Success)
            //{
            //    return BadRequest();
            //}
            if (string.IsNullOrEmpty(request.Name))
            {
                return BadRequest();
            }
            if (string.IsNullOrEmpty(request.Surname))
            {
                return BadRequest();
            }
            if (string.IsNullOrEmpty(request.Patronymic))
            {
                return BadRequest();
            }
            var resultLink = "";
            var user = _context.Users.FirstOrDefault(x => x.Name == request.Name && x.Surname == request.Surname && x.Patronymic == request.Patronymic);
            if (user != null)
            {
                await SmsService.SendSms(request.Phone, $"Ваша ссылка на выпуск сертификата https://signself.ru/editdata/{user.Id}");
                resultLink = $"signself.ru/editdata/{user.Id}";
            }
            if (user == null)
            {
                user = _context.Users.Add(new User
                {
                    Name = request.Name,
                    Surname = request.Surname,
                    Patronymic = request.Patronymic,
                    Phone = request.Phone,
                    BirthDate = DateTime.UtcNow,
                    RegDate = DateTime.UtcNow,
                    IssueDate = DateTime.UtcNow,
                    Serial = "-",
                    SubDivisionAddress = "-",
                    SubDivisionCode = "-",
                    BirthPlace = "-",
                    Email = "-",
                    Gender = new Gender(),
                    Inn = "-",
                    Number = "-",
                    RegAddress = "-",
                    Snils = "-",
                    Citizenship = "-",
                }).Entity;
                await SmsService.SendSms(request.Phone, $"Ваша ссылка на выпуск сертификата https://signself.ru/{user.Id}");
                resultLink = $"signself.ru/{user.Id}";
            }
            _context.Requests.Add(new Request
            {
                VerificationCenter = request.VerificationCenter,
                Created = DateTime.UtcNow,
                UserId = user.Id,
                RequestId = "0"
            });
            _context.SaveChanges();
            return Ok(resultLink);

        }
        [HttpPut("inn")]
        public async Task<IActionResult> Inn([FromBody] InnUpdateRequest request)
        {
            var validInn = Regex.Match(request.Inn, _regex.GetValue<string>("Inn"));
            if (!validInn.Success)
            {
                return BadRequest();
            }
            var user = _context.Users.FirstOrDefault(x => x.Id == request.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.Inn = request.Inn;
            _context.SaveChanges();
            return Ok();
        }
        [HttpPut("first")]
        public async Task<IActionResult> Update([FromBody] FirstUpdateRequest request)
        {
            var validPhone = Regex.Match(request.Phone, _regex.GetValue<string>("Phone"));
            if (!validPhone.Success)
            {
                return BadRequest();
            }
            var validEmail = Regex.Match(request.Email, _regex.GetValue<string>("Email"));
            var user = _context.Users.FirstOrDefault(x => x.Id == request.Id);
            if (user == null)
            {
                return NotFound();
            }
            if (string.IsNullOrEmpty(request.Surname))
            {
                return BadRequest();
            }
            if (string.IsNullOrEmpty(request.Name))
            {
                return BadRequest();
            }
            if (string.IsNullOrEmpty(request.Patronymic))
            {
                return BadRequest();
            }
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest();
            }
            user.Surname = request.Surname;
            user.Name = request.Name;
            user.Patronymic = request.Patronymic;
            user.Phone = request.Phone;
            user.Email = request.Email;
            _context.SaveChanges();
            return Ok(user);
        }
        [HttpPut("delivery")]
        public async Task<IActionResult> UpdateDelivery([FromForm] UpdateDeliveryRequest request)
        {
            try
            {
                var response = await _mediator.Send(request);
                if (!response.IsSuccessful)
                {
                    return BadRequest(response.Message);
                }
                return Ok(response.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet("delivery/{id}")]
        public async Task<IActionResult> GetDelivery([FromRoute] int id)
        {
            var delivery = _context.Deliveries.FirstOrDefault(x => x.TrackNumber == id);
            if (delivery == null)
            {
                return BadRequest();
            }
            return Ok(new
            {
                date = delivery.DeliveryDate.ToString("dd.MM.yyyy"),
                time = delivery.Time,
                address = delivery.Address,
                status = (int)delivery.Status

            });
        }
        [HttpPost("delivery")]
        public async Task<IActionResult> CreateDelivery([FromBody] CreateDeliveryRequest request)
        {
            try
            {
                var response = await _mediator.Send(request);
                if (!response.IsSuccess)
                {
                    return BadRequest(response.Message);
                }
                return Ok(response.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpPut("snils")]
        public async Task<IActionResult> Snils([FromBody] SnilsUpdateRequest request)
        {
            var validSnils = Regex.Match(request.Snils, _regex.GetValue<string>("Snils"));
            if (!validSnils.Success)
            {
                return BadRequest();
            }
            var user = _context.Users.FirstOrDefault(x => x.Id == request.Id);
            if (user == null)
            {
                return NotFound();
            }
            user.Snils = request.Snils;
            _context.SaveChanges();
            return Ok();
        }
        [HttpGet("2fa")]
        public async Task<IActionResult> Is2fa([FromQuery] Guid id)
        {
            var user = _context.Users.Include(x => x.Requests.OrderByDescending(x => x.Created)).FirstOrDefault(x => x.Id == id);
            if (user == null || user.Requests.Count(x => x.VerificationCenter == VerificationCenter.ItMonitoring) == 0)
            {
                return BadRequest();
            }
            return Ok(user.Requests.First().IsAuthenticated);
        }
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] CitizenUpdateRequest request)
        {
            //var validPhone = Regex.Match(request.Phone, _regex.GetValue<string>("Phone"));
            //if (!validPhone.Success)
            //{
            //    return BadRequest();
            //}
            //if (string.IsNullOrEmpty(request.Email)||!request.Email.Contains("@"))
            //{
            //    return BadRequest();

            //}
            var date = new DateTime();
            if (request.IssueDate.Length != 10)
            {
                return BadRequest();
            }
            if (request.BirthDate.Length != 10)
            {
                return BadRequest();
            }
            if (string.IsNullOrEmpty(request.RegAddress))
            {
                return BadRequest();
            }


            if (string.IsNullOrEmpty(request.Name))
            {
                return BadRequest();
            }
            if (string.IsNullOrEmpty(request.Surname))
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(request.Patronymic))
            {
                return BadRequest();
            }
            if (string.IsNullOrEmpty(request.Serial))
            {
                return BadRequest();
            }
            if (string.IsNullOrEmpty(request.Number))
            {
                return BadRequest();
            }
            if (string.IsNullOrEmpty(request.BirthPlace))
            {
                return BadRequest();
            }
            if (string.IsNullOrEmpty(request.SubDivisionAddress))
            {
                return BadRequest();
            }
            if (string.IsNullOrEmpty(request.SubDivisionCode) || request.SubDivisionCode == "0")
            {
                return BadRequest();
            }

            var user = _context.Users.FirstOrDefault(x => x.Id == request.Id);
            if (user == null)
            {
                return NotFound();
            }
            if (request.Snils != null)
            {
                user.Snils = request.Snils;
            }
            if (request.Inn != null)
            {
                user.Inn = request.Inn;
            }
            if (request.Phone != null)
            {
                user.Phone = request.Phone;

            }
            var gender = (Gender)Enum.ToObject(typeof(Gender), request.Gender);

            user.Name = request.Name;
            user.Surname = request.Surname;
            user.Patronymic = request.Patronymic;
            user.IssueDate = DateTime.Parse(request.IssueDate, CultureInfo.GetCultureInfo("ru-RU")).ToUniversalTime().AddHours(6);
            user.BirthDate = DateTime.Parse(request.BirthDate, CultureInfo.GetCultureInfo("ru-RU")).ToUniversalTime().AddHours(6);
            user.Serial = request.Serial;
            user.Number = request.Number;
            user.RegAddress = request.RegAddress;
            user.SubDivisionAddress = request.SubDivisionAddress;
            user.SubDivisionCode = request.SubDivisionCode;
            user.BirthPlace = request.BirthPlace;
            user.Gender = gender;
            user.Citizenship = "РФ";
            if (!string.IsNullOrEmpty(request.Citizenship))
            {
                user.Citizenship = request.Citizenship;
            }
            if (!string.IsNullOrEmpty(request.Email) && !string.IsNullOrEmpty(request.Phone))
            {
                user.Phone = request.Phone;
                user.Email = request.Email;
            }

            _context.SaveChanges();
            return Ok(user);
        }


    }

    public class CreateUserRequest
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string Phone { get; set; }
        public VerificationCenter VerificationCenter { get; set; }
    }
    public class FirstUpdateRequest
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
    public class CitizenUpdateRequest
    {
        public Guid Id { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string Serial { get; set; }
        public string Number { get; set; }
        public string RegAddress { get; set; }
        public string BirthPlace { get; set; }
        public string BirthDate { get; set; }
        public string IssueDate { get; set; }
        public string SubDivisionCode { get; set; }
        public string? Inn { get; set; }
        public string? Snils { get; set; }
        public string SubDivisionAddress { get; set; }
        public int Gender { get; set; }
        public long RegionCode { get; set; }
        public string? Citizenship { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    public class InnUpdateRequest
    {
        public Guid Id { get; set; }
        public string Inn { get; set; }
    }
    public class SnilsUpdateRequest
    {
        public Guid Id { get; set; }
        public string Snils { get; set; }
    }
    public class TestRequest
    {
        public string base64 { get; set; }
    }

}
