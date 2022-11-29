using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SelfSign.BL.Interfaces;
using SelfSign.Common.Entities;
using SelfSign.Common.RequestModels;
using SelfSign.Common.ResponseModels;
using SelfSign.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.BL.Commands
{
    public class CreateItMonitoringCommand : IRequestHandler<CreateItMonitoringRequest, CreateItMonitoringResponse>
    {
        private readonly IItMonitoringService _itMonitoring;
        private readonly ApplicationContext _context;
        public CreateItMonitoringCommand(ApplicationContext context, IItMonitoringService itMonitoring)
        {
            _context = context;
            _itMonitoring = itMonitoring;
        }
        public async Task<CreateItMonitoringResponse> Handle(CreateItMonitoringRequest request, CancellationToken cancellationToken)
        {
            var user = _context.Users.Include(x => x.Requests).FirstOrDefault(x => x.Id == request.Id);
            if (user == null || user.Requests.Count(x => x.VerificationCenter == VerificationCenter.ItMonitoring) > 0)
            {
                return new CreateItMonitoringResponse
                {
                    IsSuccessful = false,
                    Message = "User not found or exist request"
                };
            }
            var createRequest = new
            {
                OwnerType = 1,
                Contacts = new
                {
                    Email = user.Email,
                    Phone = user.Phone,
                },
                Address = new
                {
                    City = user.BirthPlace,
                    Value = user.RegAddress,
                },
                Owner = new
                {
                    Inn = user.Inn,
                    BirthDate = user.BirthDate.ToString("yyyy-MM-dd"),
                    BirthPlace = user.BirthPlace,
                    Snils = user.Snils,
                    Passport = new
                    {
                        Series = user.Serial,
                        Number = user.Number,
                        IssueDate = user.RegDate.ToString("yyyy-MM-dd"),
                        IssuingAuthorityCode = user.SubDivisionCode,
                        IssuingAuthorityName = user.SubDivisionAddress
                    },
                    FirstName = user.Name,
                    LastName = user.Surname,
                    MiddleName = user.Patronymic,
                    Gender = user.Gender,
                    CitizenshipCode = 643
                },
                TariffId = "deac4065-0433-497d-80b8-34784f261261"
            };
            var createResponse = await _itMonitoring.CreateRequest(createRequest);
            if (!createResponse.Item1)
            {
                return new CreateItMonitoringResponse
                {
                    IsSuccessful = false,
                    Message = createResponse.Item2
                };
            }

            var newRequest = new Request
            {
                Created = DateTime.Now.ToUniversalTime(),
                UserId = user.Id,
                RequestId = createResponse.Item2,
                VerificationCenter = VerificationCenter.ItMonitoring
            };
            _context.Requests.Add(newRequest);
            _context.SaveChanges();
            return new CreateItMonitoringResponse
            {
                IsSuccessful = true,
                Message = createResponse.Item2
            };
        }
    }
}
