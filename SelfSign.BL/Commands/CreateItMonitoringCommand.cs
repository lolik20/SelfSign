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
        private readonly IMediator _mediator;
        public CreateItMonitoringCommand(ApplicationContext context, IItMonitoringService itMonitoring, IMediator mediator)
        {
            _context = context;
            _itMonitoring = itMonitoring;
            _mediator = mediator;
        }
        public async Task<CreateItMonitoringResponse> Handle(CreateItMonitoringRequest request, CancellationToken cancellationToken)
        {
            var user = _context.Users.Include(x => x.Requests.OrderByDescending(x => x.Created)).FirstOrDefault(x => x.Id == request.Id);
            if (user == null || user.Requests.Count(x => x.VerificationCenter == VerificationCenter.ItMonitoring) == 0)
            {
                return new CreateItMonitoringResponse
                {
                    IsSuccessful = false,
                    Message = "Пользователь не найден или у вас другой УЦ"
                };
            }
            var requestEntity = user.Requests.First(x => x.VerificationCenter == VerificationCenter.ItMonitoring);
            var cladr = await _mediator.Send(new AddressRequest { query = user.RegAddress });

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
                    RegionCode = cladr.First().ShortKladr.ToString()
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
                        IssueDate = user.IssueDate.ToString("yyyy-MM-dd"),
                        IssuingAuthorityCode = user.SubDivisionCode,
                        IssuingAuthorityName = user.SubDivisionAddress
                    },
                    FirstName = user.Name,
                    LastName = user.Surname,
                    MiddleName = user.Patronymic,
                    Gender = user.Gender,
                    CitizenshipCode = 643
                },
                TariffId = "5f278abe-1ee9-4106-9bfb-bd90b8745938"
            };
            var result = Tuple.Create(false, "Ошибка создания заявки");
            if (Guid.TryParse(requestEntity.RequestId, out Guid guid))
            {
                result = await _itMonitoring.UpdateRequest(createRequest, requestEntity.RequestId);
            }
            else
            {
                result = await _itMonitoring.CreateRequest(createRequest);
            }
            if (!result.Item1)
            {
                return new CreateItMonitoringResponse
                {
                    IsSuccessful = false,
                    Message = result.Item2
                };
            }
            if (result.Item2 != null)
            {
                requestEntity.RequestId = result.Item2;
                _context.SaveChanges();
            }
            return new CreateItMonitoringResponse
            {
                IsSuccessful = true,
                Message = result.Item2
            };
        }
    }
}
