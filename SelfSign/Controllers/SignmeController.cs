using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SelfSign.Common.Entities;
using SelfSign.Common.RequestModels;
using SelfSign.DAL;

namespace SelfSign.Controllers
{

    public class SignmeController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        public SignmeController(IMediator mediator)
        {

            _mediator = mediator;
        }

        [HttpGet("request")]
        public async Task<IActionResult> Request([FromQuery]SignMeRequest request)
        {
            var response = await _mediator.Send(request);
            if (!response.IsSuccessful)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Message);
        }

    }

}
