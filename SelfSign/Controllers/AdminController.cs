using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelfSign.BL.Interfaces;
using SelfSign.DAL;
using SelfSign.Utils;

namespace SelfSign.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IFileService _fileService;
        private readonly IItMonitoringService _itMonitoring;
        private readonly IConfiguration _configuration;
        public AdminController(ApplicationContext context, IFileService fileService, IItMonitoringService itMonitoring, IConfiguration configuration)
        {
            _context = context;
            _fileService = fileService;
            _itMonitoring = itMonitoring;
            _configuration = configuration;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("document")]
        public async Task<IActionResult> GetDocument([FromQuery] Guid id)
        {
            var document = _context.Documents.FirstOrDefault(x => x.Id == id);
            if (document == null)
            {
                return NotFound();
            }
            var result = _fileService.GetBase64(document.FileUrl);
            return Ok(new
            {
                type = document.FileUrl.Split(".")[1] == "jpg" ? "image/jpeg" : "application/pdf",
                base64 = result
            });
        }
        [Authorize(Roles ="Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var result = _context.Users.Select(x => new
            {
                Id = x.Id,
                FIO = $"{x.Surname} {x.Name} {x.Patronymic}",
                RegAddress = x.RegAddress,
                PassportNumber = $"{x.Serial} {x.Number}",
                BirthPlace = x.BirthPlace,
                BirthDate = $"{x.BirthDate.ToString("dd.MM.yyyy")}",
                IssueDate = $"{x.IssueDate.ToString("dd.MM.yyyy")}",
                SubdivisionCode = x.SubDivisionCode,
                SubdivisionAddress = x.SubDivisionAddress,
                Snils = x.Snils,
                Inn = x.Inn,
                Email = x.Email,
                Phone = x.Phone,
                RegDate = x.RegDate.ToString("dd.MM.yyyy")
            }).ToList();
            return Ok(result);
        }
        [HttpGet("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _context.UserData.FirstOrDefault(x => x.UserName == username);   
            if (user == null)
            {
                return NotFound();
            }
            var isValid = Argon2.Verify(user.PasswordHash, password, _configuration.GetSection("Argon")["Key"]);
            if (isValid)
            {
                var token = Jwt.GetToken(user.Id, user.Role);
                return Ok(token);
            }
            return NotFound();
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("user")]
        public async Task<IActionResult> GetUserData([FromQuery] Guid id)
        {
            var user = _context.Users.Include(x => x.Requests).ThenInclude(x => x.Documents).Include(x => x.Requests).ThenInclude(x => x.Deliveries).Include(x => x.Requests).ThenInclude(x => x.History).FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var result = new
            {
                Id = user.Id,
                FIO = $"{user.Surname} {user.Name} {user.Patronymic}",
                RegAddress = user.RegAddress,
                PassportNumber = $"{user.Serial} {user.Number}",
                BirthPlace = user.BirthPlace,
                BirthDate = $"{user.BirthDate.ToString("dd.MM.yyyy")}",
                IssueDate = $"{user.IssueDate.ToString("dd.MM.yyyy")}",
                SubdivisionCode = user.SubDivisionCode,
                SubdivisionAddress = user.SubDivisionAddress,
                Snils = user.Snils,
                Inn = user.Inn,
                Email = user.Email,
                Phone = user.Phone,
                Documents = user.Requests.SelectMany(x => x.Documents, (parent, child) => new
                {
                    Id = child.Id,
                    Type = (int)child.DocumentType,
                    Created = child.Created.ToString("dd.MM.yyyy")
                }),
                Requests = user.Requests.Select(x => new
                {
                    Id = x.Id,
                    Created = x.Created.ToString("dd.MM.yyyy"),
                    Center = (int)x.VerificationCenter,
                    RequestId = x.RequestId,
                    IsAuthenticated = x.IsAuthenticated,
                    Status = (Guid.TryParse(x.RequestId, out Guid guid) ? _itMonitoring.GetStatus(x.RequestId).GetAwaiter().GetResult() : 0)
                }),
                Deliveries = user.Requests.SelectMany(x => x.Deliveries, (parent, child) => new
                {
                    Id = child.Id,
                    Created = child.Created.ToString("dd.MM.yyyy"),
                    Delivery = child.DeliveryDate.ToString("dd.MM.yyyy"),
                    Time = child.Time,
                    Cladr = child.Cladr,
                    Center = (int)parent.VerificationCenter,
                    Address = child.Address,
                    Status = child.Status,
                    TrackNumber = child.TrackNumber
                }),
                History = user.Requests.SelectMany(x => x.History, (parent, child) => new
                {
                    Id = child.Id,
                    RequestId = parent.Id,
                    Event = child.Event,
                    Created = child.Created.ToString("dd.MM.yyyy")
                })
            };
            return Ok(result);
        }
    }
}
