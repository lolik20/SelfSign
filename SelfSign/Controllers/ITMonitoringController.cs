using Microsoft.AspNetCore.Mvc;

namespace SelfSign.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ITMonitoringController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;
        public ITMonitoringController(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            Initial(configuration);
        }
        private void Initial(IConfiguration configuration)
        {
        }
        //[HttpPost("request")]
        //public Task<IActionResult> Request()
        //{
            
        //}
        //[HttpPost("twofactor")]
        //public Task<IActionResult> TwoFactor()
        //{

        //}
        //[HttpPost("confirmation")]
        //public Task<IActionResult> Comfirmation()
        //{

        //}
    }
    public enum ITMonitoringMethods
    {
        Request =0,
        TwoFactor =1,
        Confirmation=2
    }

}
