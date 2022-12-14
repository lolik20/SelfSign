using MediatR;
using Microsoft.EntityFrameworkCore;
using SelfSign.BL.Interfaces;
using SelfSign.BL.Services;
using SelfSign.Common.RequestModels;
using SelfSign.Common.ResponseModels;
using SelfSign.DAL;


namespace SelfSign.BL.Commands
{
    public class ItMonitoringPassportCommand : IRequestHandler<ItMonitoringPassportRequest, ItMonitoringPassportResponse>
    {
        private readonly ApplicationContext _context;
        private readonly IItMonitoringService _itMonitoring;
        private readonly IFileService _fileService;
        private readonly IHistoryService _historyService;
        public ItMonitoringPassportCommand(ApplicationContext context, IItMonitoringService itMonitoring, IFileService fileService, IHistoryService historyService)
        {
            _context = context;
            _itMonitoring = itMonitoring;
            _fileService = fileService;
            _historyService = historyService;
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
            var preStatus = await _itMonitoring.GetStatus(requestEntity.RequestId);
            if (preStatus >= 3)
            {
                return new ItMonitoringPassportResponse
                {
                    IsSuccessful = true,
                    Message = "Успешная идентификация личности"
                };
            }
            if (preStatus == 1)
            {
                var documentEntity = requestEntity.Documents.First(x => x.DocumentType == Common.Entities.DocumentType.Passport);
                var response = await _itMonitoring.UploadDocuments(requestEntity.RequestId, _fileService.GetDocument(documentEntity.FileUrl), Common.Entities.DocumentType.Passport, "passport", "jpg", "image/jpeg");
                var confirmation = await _itMonitoring.Confirmation(requestEntity.RequestId);
                await _historyService.AddHistory(requestEntity.Id, "Загрузка паспорта в УЦ");
                await SmsService.SendSms(user.Phone, "Идет проверка в СМЭВ. Ожидайте SMS о готовности");
            }
            for (int i = 0; i < 50; i++)
            {
                await Task.Delay(30000);
                var status = await _itMonitoring.GetStatus(requestEntity.RequestId);
                if (status == 4)
                {
                    await _historyService.AddHistory(requestEntity.Id, "Успешная идентификация паспорта");
                    _context.SaveChanges();
                    await SmsService.SendSms(user.Phone, "Проверка пройдена. Продолжите выпуск подписи в SignSelf");

                    return new ItMonitoringPassportResponse
                    {
                        IsSuccessful = true,
                        Message = "Успешная идентификация личности"
                    };
                }
                if (status == 1)
                {
                    var comment = await _itMonitoring.GetComment(requestEntity.RequestId);
                    await _historyService.AddHistory(requestEntity.Id, $"Идентификация личности не пройдена: {comment}");
                    _context.SaveChanges();
                    await SmsService.SendSms(user.Phone, $"Проверка в СМЭВ не пройдена: {comment}");
                    return new ItMonitoringPassportResponse
                    {
                        IsSuccessful = false,
                        Message = $"Идентификация личности не пройдена: {comment}"
                    };
                }
            }
            var error = await _itMonitoring.GetComment(requestEntity.RequestId);
            return new ItMonitoringPassportResponse
            {
                IsSuccessful = false,
                Message = $"Превышено время ожидания, обратитесь в тех поддержку"
            };


        }
    }
}
