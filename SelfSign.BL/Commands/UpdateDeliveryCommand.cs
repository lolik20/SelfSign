using MediatR;
using Microsoft.EntityFrameworkCore;
using SelfSign.BL.Interfaces;
using SelfSign.BL.Services;
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
    public class UpdateDeliveryCommand : IRequestHandler<UpdateDeliveryRequest, UpdateDeliveryResponse>
    {
        private readonly ApplicationContext _context;
        private readonly IFileService _fileService;
        private readonly IItMonitoringService _itMonitoring;
        private readonly IHistoryService _historyService;
        public UpdateDeliveryCommand(ApplicationContext context, IFileService fileService, IItMonitoringService itMonitoring, IHistoryService historyService)
        {
            _context = context;
            _fileService = fileService;
            _itMonitoring = itMonitoring;
            _historyService = historyService;
        }

        public async Task<UpdateDeliveryResponse> Handle(UpdateDeliveryRequest request, CancellationToken cancellationToken)
        {
            var deliveryEntity = _context.Deliveries.Include(x => x.Request).ThenInclude(x => x.User).FirstOrDefault(x => x.Id == request.Id);
            if (deliveryEntity == null)
            {
                return new UpdateDeliveryResponse
                {
                    IsSuccessful = false,
                    Message = "Delivery not found"
                };
            }
            var requestEntity = deliveryEntity.Request;
            if (requestEntity == null)
            {
                return new UpdateDeliveryResponse
                {
                    IsSuccessful = false,
                    Message = "Request not found"
                };
            }
            var documents = _context.Documents.Where(x => x.RequestId == requestEntity.Id);

            if (request.StatementPhoto != null)
            {
                var statementPhotoEntity = _context.Documents.Add(new Common.Entities.Document
                {
                    Created = DateTime.UtcNow,
                    DocumentType = Common.Entities.DocumentType.PhotoWithStatement,
                    RequestId = requestEntity.Id
                });

                var statementPhotoUrl = await _fileService.AddFile(request.StatementPhoto, requestEntity.User.Id, statementPhotoEntity.Entity.Id, "jpg");
                statementPhotoEntity.Entity.FileUrl = statementPhotoUrl;
            }
            if (request.PassportScan != null)
            {
                var passportScanEntity = documents.FirstOrDefault(x => x.DocumentType == Common.Entities.DocumentType.Passport);
                var passportScanUrl = await _fileService.AddFile(request.PassportScan, requestEntity.User.Id, passportScanEntity.Id, "jpg");
                passportScanEntity.FileUrl = passportScanUrl;
                passportScanEntity.Created = DateTime.UtcNow;
            }

            if (request.StatementScan != null)
            {
                var statementScanEntity = documents.FirstOrDefault(x => x.DocumentType == Common.Entities.DocumentType.Statement);
                var statementScanUrl = await _fileService.AddFile(request.StatementScan, requestEntity.User.Id, statementScanEntity.Id, "jpg");
                statementScanEntity.FileUrl = statementScanUrl;
                statementScanEntity.Created = DateTime.UtcNow;
            }
            _context.SaveChanges();
            switch (requestEntity.VerificationCenter)
            {
                case Common.Entities.VerificationCenter.ItMonitoring:

                    var isStatement = await _itMonitoring.UploadDocuments(requestEntity.RequestId, _fileService.FromFile(request.StatementScan), Common.Entities.DocumentType.Statement, "statementScan", "jpg", "image/jpeg");
                    var isPassport = await _itMonitoring.UploadDocuments(requestEntity.RequestId, _fileService.FromFile(request.PassportScan), Common.Entities.DocumentType.Passport, "passport", "jpg", "image/jpeg");
                    if (isStatement && isPassport)
                    {
                        var isConfirmed = await _itMonitoring.Confirmation(requestEntity.RequestId);

                    }

                    break;
            }
            deliveryEntity.Status = Common.Entities.DeliveryStatus.Completed;
            await _historyService.AddHistory(requestEntity.Id, "Прикрепление документов курьерами");
            _context.SaveChanges();
            switch (requestEntity.VerificationCenter)
            {
                case Common.Entities.VerificationCenter.ItMonitoring:

                    for (int i = 0; i < 30; i++)
                    {
                        await Task.Delay(30000);
                        var status = await _itMonitoring.GetStatus(requestEntity.RequestId);
                        if (status == 10)
                        {
                            await SmsService.SendSms(deliveryEntity.PhoneNumber, "Ваш сертификат выпущен. Зайдите в приложение MYDSS");
                            await _historyService.AddHistory(requestEntity.Id, "Отправка SMS уведомления о выпуске сертификата");

                        }
                    }
                    break;
                case Common.Entities.VerificationCenter.SignMe:

                    for (int i = 0; i < 30; i++)
                    {
                        await Task.Delay(30000);
                        var status = await _itMonitoring.GetStatus(requestEntity.RequestId);
                        if (status == 10)
                        {
                            await SmsService.SendSms(deliveryEntity.PhoneNumber, "Ваш сертификат выпущен. Зайдите в приложение SignMe");
                            await _historyService.AddHistory(requestEntity.Id, "Отправка SMS уведомления о выпуске сертификата");

                        }
                    }
                    break;
            }
            _context.SaveChanges();
            return new UpdateDeliveryResponse
            {
                IsSuccessful = true,
                Message = "Documents updated"
            };
        }
    }
}
