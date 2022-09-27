﻿using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> CreateUser([FromBody] User request)
        {
            var user = _context.Users.FirstOrDefault(x=>x.Id==request.Id);
            if (user == null)
            {
                _context.Users.Add(new User
                {
                    Id = request.Id,
                    Name=request.Name,
                    Surname=request.Surname,
                    Patronymic=request.Patronymic
                });
                return Ok();
            }
            return BadRequest();
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
        public Guid Id { get; set; }
        public User User { get; set; }
    }

}