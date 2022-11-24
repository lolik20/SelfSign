using Microsoft.AspNetCore.Mvc;
using SelfSign.DAL;

namespace SelfSign.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GosKeyController:ControllerBase
    {
        private readonly ApplicationContext _context;
        public GosKeyController(ApplicationContext context)
        {
            _context = context;
        }
        //public async Task<IActionResult> Authorize()
        //{

        //}
    }
}
