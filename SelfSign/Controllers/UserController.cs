using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelfSign.Entities;
using System.Globalization;
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
        public UserController(ApplicationContext context,IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _regex = _configuration.GetSection("regex");
        }
        [HttpGet]
        public async Task<IActionResult> GetUser([FromQuery] Guid id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var validPhone = Regex.Match(request.Phone, _regex.GetValue<string>("Phone"));
            if (!validPhone.Success)
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

            var user = _context.Users.FirstOrDefault(x => x.Name == request.Name && x.Surname == request.Surname && x.Patronymic == request.Patronymic && x.Phone == request.Phone);
            if (user == null)
            {
                var newEntity = _context.Users.Add(new User
                {
                    Name = request.Name,
                    Surname = request.Surname,
                    Patronymic = request.Patronymic,
                    Phone = request.Phone,
                    BirthDate = DateTime.Now.ToUniversalTime(),
                    RegDate = DateTime.Now.ToUniversalTime(),
                    IssueDate = DateTime.Now.ToUniversalTime(),

                    SignatureType = request.SignatureType,
                    Serial = "",
                    SubDivisionAddress = "",
                    SubDivisionCode = "",
                    BirthPlace = "",
                    Email = "",
                    Gender = new Gender(),
                    Inn = "",
                    Number = "",
                    RegAddress = "",
                    Snils = "",
                    Citizenship = "",


                });
                _context.SaveChanges();
                return Ok(newEntity.Entity.Id);
            }
            return BadRequest(user.Id);
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
            var date =new  DateTime();
            if (DateTime.TryParse(request.IssueDate, out date)&&request.IssueDate.Length!=10)
            {
                return BadRequest();
            }
            if (DateTime.TryParse(request.BirthDate, out date)&&request.BirthDate.Length!=10)
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
            if (string.IsNullOrEmpty(request.SubDivisionCode)||request.SubDivisionCode=="0")
            {
                return BadRequest();
            }
            var user = _context.Users.FirstOrDefault(x => x.Id == request.Id);
            if (user == null)
            {
                return NotFound();
            }
            var gender = (Gender)Enum.ToObject(typeof(Gender), request.Gender);
            user.Name = request.Name;
            user.Surname = request.Surname;
            user.Patronymic = request.Patronymic;
            user.IssueDate = DateTime.Parse(request.IssueDate, CultureInfo.GetCultureInfo("ru-RU")).ToUniversalTime();
            user.BirthDate = DateTime.Parse(request.BirthDate, CultureInfo.GetCultureInfo("ru-RU")).ToUniversalTime();
            user.Serial = request.Serial;
            user.Number = request.Number;
            user.RegAddress = request.RegAddress;
            user.SubDivisionAddress = request.SubDivisionAddress;
            user.SubDivisionCode = request.SubDivisionCode;
            user.BirthPlace = request.BirthPlace;
            user.Gender = gender;
            user.RegionCode = request.RegionCode;

            user.Citizenship = "RU";
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
        public SignatureType SignatureType { get; set; }
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


}
