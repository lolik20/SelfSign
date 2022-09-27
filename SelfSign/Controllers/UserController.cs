using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelfSign.Entities;

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
                    BirthDate = DateTime.Now,
                    RegDate = DateTime.Now,
                    IssueDate = DateTime.Now,

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

    }
    public class ForeignerUpdateRequest
    {

    }
}
