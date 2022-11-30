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
    public class ItMonitoringBlankQuery : IRequestHandler<ItMonitoringBlankRequest, ItMonitoringBlankResponse>
    {
        private readonly ApplicationContext _context;
        private readonly IFileService _fileService;
        private readonly IItMonitoringService _itMonitoring;
        public ItMonitoringBlankQuery(ApplicationContext context, IFileService fileService, IItMonitoringService itMonitoring)
        {
            _context = context;
            _fileService = fileService;
            _itMonitoring = itMonitoring;
        }

        public async Task<ItMonitoringBlankResponse> Handle(ItMonitoringBlankRequest request, CancellationToken cancellationToken)
        {
            var user = _context.Users.Include(x => x.Requests.OrderByDescending(x => x.Created)).ThenInclude(x => x.Documents).FirstOrDefault(x => x.Id == request.Id);
            if (user == null || user.Requests.Count(x => x.VerificationCenter == Common.Entities.VerificationCenter.ItMonitoring) == 0)
            {
                return new ItMonitoringBlankResponse
                {
                    IsSuccessful = false,
                    Message = "Пользователь не найден или нет заявки "
                };
            }
            var requestEntity = user.Requests.First(x => x.VerificationCenter == Common.Entities.VerificationCenter.ItMonitoring);

            var documentEntity = requestEntity.Documents.FirstOrDefault(x => x.DocumentType == Common.Entities.DocumentType.Statement);
            if (documentEntity != null)
            {
                return new ItMonitoringBlankResponse
                {
                    IsSuccessful = true,
                    Message = "Бланк получен"
                };
            }
            var statement = await _itMonitoring.GetDocument(requestEntity.RequestId, Common.Entities.DocumentType.Statement);
            if (statement == null)
            {
                return new ItMonitoringBlankResponse
                {
                    IsSuccessful = false,
                    Message = "Ошибка получения бланка заявления"
                };
            }
            var newDocument = _context.Documents.Add(
                new Common.Entities.Document
                {
                    Created = DateTime.UtcNow,
                    DocumentType = Common.Entities.DocumentType.Statement,
                    RequestId = requestEntity.Id
                });
            var fileUrl = await _fileService.AddFile(statement, user.Id, newDocument.Entity.Id, "pdf");
            newDocument.Entity.FileUrl = fileUrl;
            _context.SaveChanges();
            return new ItMonitoringBlankResponse
            {
                IsSuccessful = true,
                Message = "Бланк заявления получен"
            };
        }
    }
}
