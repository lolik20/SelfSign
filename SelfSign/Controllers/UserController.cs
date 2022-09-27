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
            var user = _context.Users.FirstOrDefault(x=>x.Name==request.Name&&x.Surname==request.Surname&&x.Patronymic==request.Patronymic&&x.Phone==request.Phone);
            if (user == null)
            {
               var newEntity=  _context.Users.Add(new User
                {
                    Name=request.Name,
                    Surname=request.Surname,
                    Patronymic=request.Patronymic,
                    Phone=request.Phone,
                    BirthDate=default,
                    RegDate=default,
                    SignatureType=request.SignatureType,
                    Serial="",
                    SubDivisionAddress="",
                    SubDivisionCode="",
                    BirthPlace="",
                    Email="",
                    Gender="",
                    Inn="",
                    Number="",
                    RegAddress="",
                    Snils=""
                   
                });
                _context.SaveChanges();
                return Ok(newEntity.Entity.Id);
            }
            return BadRequest(user.Id);
        }
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();
            return Ok();
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

}
