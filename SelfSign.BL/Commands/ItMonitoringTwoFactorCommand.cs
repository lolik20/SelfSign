using MediatR;
using Microsoft.EntityFrameworkCore;
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
    public class ItMonitoringTwoFactorCommand : IRequestHandler<ItMonitoringTwoFactorRequest, ItMonitoringTwoFactorResponse>
    {
        private readonly ApplicationContext _context;
        private readonly IItMonitoringService _itMonitoring;
        public ItMonitoringTwoFactorCommand(ApplicationContext context, IItMonitoringService itMonitoring)
        {
            _context = context;
            _itMonitoring = itMonitoring;
        }

        public async Task<ItMonitoringTwoFactorResponse> Handle(ItMonitoringTwoFactorRequest request, CancellationToken cancellationToken)
        {
            var user = _context.Users.Include(x => x.Requests.OrderByDescending(x => x.Created)).FirstOrDefault(x => x.Id == request.Id);
            if (user == null||user.Requests.Count(x=>x.VerificationCenter==VerificationCenter.ItMonitoring)==0)
            {
                return new ItMonitoringTwoFactorResponse
                {
                    IsSuccessful = false
                };
            }
            var requestEntity = user.Requests.First(x => x.VerificationCenter == VerificationCenter.ItMonitoring);
            var requestObject = new
            {
                Dss2fa = 2,
                Codeword = request.Alias
            };
            var isTwoFactor = await _itMonitoring.TwoFactor(requestEntity.RequestId, requestObject);
            if (!isTwoFactor)
            {
                return new ItMonitoringTwoFactorResponse { IsSuccessful = false };
            }
            requestEntity.IsAuthenticated = true;
            _context.SaveChanges();
            return new ItMonitoringTwoFactorResponse { IsSuccessful = true };
        }
    }
}
