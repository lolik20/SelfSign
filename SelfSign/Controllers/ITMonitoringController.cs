using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SelfSign.BL.Services;
using SelfSign.Common.Entities;
using SelfSign.Common.RequestModels;
using SelfSign.DAL;

namespace SelfSign.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ITMonitoringController : ControllerBase
    {

        private readonly IMediator _mediator;
        public ITMonitoringController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet("request")]
        public async Task<IActionResult> Request([FromQuery] CreateItMonitoringRequest request)
        {
            var response = await _mediator.Send(request);
            if (!response.IsSuccessful)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Message);
        }

        [HttpGet("twofactor")]
        public async Task<IActionResult> TwoFactor([FromQuery] ItMonitoringTwoFactorRequest request)
        {
            var response = await _mediator.Send(request);
            if (!response.IsSuccessful)
            {
                return BadRequest();
            }
            return Ok();

        }
        [HttpGet("documents/upload")]
        public async Task<IActionResult> SendDocuments([FromQuery] ItMonitoringPassportRequest request)
        {
            var response = await _mediator.Send(request);
            if (response.IsSuccessful)
            {
                return Ok(response.Message);
            }
            return BadRequest(response.Message);
        }
        [HttpGet("confirmation")]
        public async Task<IActionResult> Confirmation([FromQuery] ItMonitoringConfirmationRequest request)
        {
            var response = await _mediator.Send(request);
            if (response.IsSuccessful)
            {
                return Ok();
            }
            return BadRequest(response.Message);
        }
        [HttpGet("blank")]
        public async Task<IActionResult> GetBlank([FromQuery] ItMonitoringBlankRequest request)
        {

            var response = await _mediator.Send(request);
            if (response.IsSuccessful)
            {
                return Ok(response.Message);
            }
            return BadRequest(response.Message);
        }


    }

}
