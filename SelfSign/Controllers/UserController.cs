using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelfSign.Entities;
using System.Globalization;

namespace SelfSign.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationContext _context;
        public UserController(ApplicationContext context)
        {
            _context = context;
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
                    Gender = "",
                    Inn = "",
                    Number = "",
                    RegAddress = "",
                    Snils = ""

                });
                _context.SaveChanges();
                return Ok(newEntity.Entity.Id);
            }
            return BadRequest(user.Id);
        }
        [HttpPut("inn")]
        public async Task<IActionResult> Inn([FromBody]InnUpdateRequest request)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == request.Id);
            if (user == null)
            {
                return NotFound();
            }
            user.Inn = request.Inn;
            _context.SaveChanges();
            return Ok();
        }
        [HttpPut("snils")]
        public async Task<IActionResult> Snils([FromBody] SnilsUpdateRequest request)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == request.Id);
            if (user == null)
            {
                return NotFound();
            }
            user.Snils = request.Snils;
            _context.SaveChanges();
            return Ok();
        }
        [HttpPut("first")]
        public async Task<IActionResult> Update([FromBody] FirstUpdateRequest request)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == request.Id);
            if (user == null)
            {
                return NotFound();
            }
            user.Surname = request.Surname;
            user.Name = request.Name;
            user.Patronymic = request.Patronymic;
            user.Phone = request.Phone;
            user.Email = request.Email;
            _context.SaveChanges();
            return Ok(user);
        }
        [HttpPut("citizen")]
        public async Task<IActionResult> Update([FromBody] CitizenUpdateRequest request)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == request.Id);
            if (user == null)
            {
                return NotFound();
            }
            var gender = new Gender();
            Enum.TryParse(request.Gender,out gender);
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
            user.Citizenship = "RU";
            _context.SaveChanges();
            return Ok(user);
        }
        [HttpPut("foreigner")]
        public async Task<IActionResult> Update([FromBody] ForeignerUpdateRequest request)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == request.Id);
            if (user == null)
            {
                return NotFound();
            }
            var gender = new Gender();
            
            Enum.TryParse(request.Gender, out gender);

            user.IssueDate = DateTime.Parse(request.IssueDate, CultureInfo.GetCultureInfo("ru-RU")).ToUniversalTime(); 
            user.BirthDate = DateTime.Parse(request.BirthDate, CultureInfo.GetCultureInfo("ru-RU")).ToUniversalTime();
            user.Serial = request.Serial;
            user.Number = request.Number;
            user.RegAddress = request.RegAddress;
            user.SubDivisionAddress = request.SubDivisionAddress;
            user.SubDivisionCode = request.SubDivisionCode;
            user.BirthPlace = request.BirthPlace;
            user.Citizenship = request.Citizenship;
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
        public string Gender { get; set; }

    }
    public class ForeignerUpdateRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string Serial { get; set; }
        public string Number { get; set; }
        public string RegAddress { get; set; }
        public string BirthPlace { get; set; }
        public string BirthDate { get; set; }
        public string IssueDate { get; set; }
        public string SubDivisionCode { get; set; }
        public string SubDivisionAddress { get; set; }
        public string Gender { get; set; }
        public string Citizenship { get; set; }
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
