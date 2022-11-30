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

namespace SelfSign.BL.Commands
{
    public class ItMonitoringPassportCommand : IRequestHandler<ItMonitoringPassportRequest, ItMonitoringPassportResponse>
    {
        private readonly ApplicationContext _context;
        private readonly IItMonitoringService _itMonitoring;
        private readonly IFileService _fileService;
        public ItMonitoringPassportCommand(ApplicationContext context, IItMonitoringService itMonitoring, IFileService fileService)
        {
            _context = context;
            _itMonitoring = itMonitoring;
            _fileService = fileService;
        }

        public async Task<ItMonitoringPassportResponse> Handle(ItMonitoringPassportRequest request, CancellationToken cancellationToken)
        {
            var user = _context.Users.Include(x => x.Requests.OrderByDescending(x => x.Created)).ThenInclude(x => x.Documents).FirstOrDefault(x => x.Id == request.Id);
            if (user == null || user.Requests.Count(x => x.VerificationCenter == Common.Entities.VerificationCenter.ItMonitoring) == 0)
            {
                return new ItMonitoringPassportResponse
                {
                    IsSuccessful = false,
                    Message = "Пользователь не найден или нет заявки"
                };
            }
            var requestEntity = user.Requests.First(x => x.VerificationCenter == Common.Entities.VerificationCenter.ItMonitoring);
            var documentEntity = requestEntity.Documents.First(x => x.DocumentType == Common.Entities.DocumentType.Passport);
            var response = await _itMonitoring.UploadDocuments(requestEntity.RequestId, _fileService.GetDocument(documentEntity.FileUrl), Common.Entities.DocumentType.Passport, "passport", "jpg", "image/jpeg");
            var confirmation = await _itMonitoring.Confirmation(requestEntity.RequestId);
            return new ItMonitoringPassportResponse
            {
                IsSuccessful = true,
                Message = "Паспорт загружен"
            };
        }
    }
}
