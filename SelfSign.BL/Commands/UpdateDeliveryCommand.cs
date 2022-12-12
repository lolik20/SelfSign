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
        public UpdateDeliveryCommand(ApplicationContext context, IFileService fileService, IItMonitoringService itMonitoring)
        {
            _context = context;
            _fileService = fileService;
            _itMonitoring = itMonitoring;
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

            //var statementPhotoEntity = _context.Documents.Add(new Common.Entities.Document
            //{
            //    Created = DateTime.UtcNow,
            //    DocumentType = Common.Entities.DocumentType.PhotoWithStatement,
            //    RequestId = requestEntity.Id
            //});
            //var statementPhotoUrl = await _fileService.AddFile(request.StatementPhoto, requestEntity.User.Id, statementPhotoEntity.Entity.Id, "jpg");
            //statementPhotoEntity.Entity.FileUrl = statementPhotoUrl;

            var passportScanEntity = documents.FirstOrDefault(x => x.DocumentType == Common.Entities.DocumentType.Passport);
            var passportScanUrl = await _fileService.AddFile(request.PassportScan, requestEntity.User.Id, passportScanEntity.Id, "jpg");
            passportScanEntity.FileUrl = passportScanUrl;
            passportScanEntity.Created = DateTime.UtcNow;

            var statementScanEntity = documents.FirstOrDefault(x => x.DocumentType == Common.Entities.DocumentType.Statement);
            var statementScanUrl = await _fileService.AddFile(request.StatementScan, requestEntity.User.Id, statementScanEntity.Id, "jpg");
            statementScanEntity.FileUrl = statementScanUrl;
            statementScanEntity.Created = DateTime.UtcNow;
            _context.SaveChanges();
            switch (requestEntity.VerificationCenter)
            {
                case Common.Entities.VerificationCenter.ItMonitoring:

                    var isStatement = await _itMonitoring.UploadDocuments(requestEntity.RequestId, _fileService.FromFile(request.StatementScan), Common.Entities.DocumentType.Statement, "statementScan", "jpg", "image/jpeg");
                    var isPassport = await _itMonitoring.UploadDocuments(requestEntity.RequestId, _fileService.FromFile(request.PassportScan), Common.Entities.DocumentType.Passport, "passport", "jpg", "image/jpeg");
                    if (isStatement && isPassport)
                    {
                        var isConfirmed = await _itMonitoring.Confirmation(requestEntity.RequestId);
                        //if (isConfirmed)
                        //{
                        //    await _itMonitoring.SimulateConfirmation(requestEntity.RequestId);
                        //}
                    }

                    break;
            }
            deliveryEntity.Status = Common.Entities.DeliveryStatus.Completed;
            _context.SaveChanges();
            return new UpdateDeliveryResponse { IsSuccessful = true, Message = "Documents updated" };
        }
    }
}
