using MediatR;
using Microsoft.EntityFrameworkCore;
using SelfSign.BL.Interfaces;
using SelfSign.Common.RequestModels;
using SelfSign.Common.ResponseModels;
using SelfSign.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.BL.Queries
{
    public class ItMonitoringConfirmationQuery : IRequestHandler<ItMonitoringConfirmationRequest, ItMonitoringConfirmationResponse>
    {
        private readonly ApplicationContext _context;
        private readonly IItMonitoringService _itMonitoring;
        public ItMonitoringConfirmationQuery(ApplicationContext context, IItMonitoringService itMonitoring)
        {
            _context = context;
            _itMonitoring = itMonitoring;
        }

        public async Task<ItMonitoringConfirmationResponse> Handle(ItMonitoringConfirmationRequest request, CancellationToken cancellationToken)
        {
            var user = _context.Users.Include(x => x.Requests.OrderByDescending(x => x.Created)).FirstOrDefault(x => x.Id == request.Id);
            if (user == null || user.Requests.Count(x => x.VerificationCenter == Common.Entities.VerificationCenter.ItMonitoring) == 0)
            {
                return new ItMonitoringConfirmationResponse
                {
                    IsSuccessful = false,
                    Message="Пользователь не существует или нет заявки"
                };
            }
            var result = await _itMonitoring.Confirmation(user.Requests.First(x => x.VerificationCenter == Common.Entities.VerificationCenter.ItMonitoring).RequestId);
            
            return new ItMonitoringConfirmationResponse { IsSuccessful = result };
        }
    }
}
