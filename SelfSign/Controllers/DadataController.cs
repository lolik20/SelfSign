
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SelfSign.Common.RequestModels;

namespace SelfSign.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DadataController : ControllerBase
    {
        private readonly IMediator _mediator;
        public DadataController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("address")]
        public async Task<IActionResult> Address([FromQuery]AddressRequest request)
        {
            try
            {
                var response =await _mediator.Send(request);
                if (response.Count == 0)
                {
                    return NotFound();
                }
                return Ok(response);
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet("issuedby")]
        public async Task<IActionResult> IssuedBy([FromQuery]IssuedByRequest request)
        {
            try
            {
                var response = await _mediator.Send(request);
                if (response.Count == 0)
                {
                    return NotFound();
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }

    enum DadataMethod
    {
        Address = 0,
        Fms = 1
    }


}

