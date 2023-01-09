using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class CreateSignMeCommand : IRequestHandler<SignMeRequest, SignMeResponse>
    {
        private readonly ApplicationContext _context;
        private readonly IMediator _mediator;
        private readonly ISignmeService _signmeService;
        private readonly IFileService _fileService;
        public CreateSignMeCommand(ApplicationContext context, IMediator mediator, ISignmeService signmeService, IFileService fileService)
        {
            _context = context;
            _mediator = mediator;
            _signmeService = signmeService;
            _fileService = fileService;
        }

        public async Task<SignMeResponse> Handle(SignMeRequest request, CancellationToken cancellationToken)
        {

            var user = _context.Users.Include(x => x.Requests.OrderByDescending(x => x.Created)).FirstOrDefault(x => x.Id == request.Id);
            if (user == null || user.Requests.Count(x => x.VerificationCenter == VerificationCenter.SignMe) == 0)
            {
                return new SignMeResponse
                {
                    IsSuccessful = false
                };
            }
            var requestEntity = user.Requests.First();
            var precheck = await _signmeService.PreCheck(user);
            var cladr = await _mediator.Send(new AddressRequest { query = user.RegAddress });
            if (cladr.Count == 0)
            {
                return new SignMeResponse
                {
                    IsSuccessful = false,
                    Message = "Неизвестный адрес регистрации"
                };
            }
            var createResponse = await _signmeService.Create(user, cladr.First().ShortKladr.ToString());
            if (!createResponse.Item1 && requestEntity.Qr == null)
            {
                return new SignMeResponse
                {
                    IsSuccessful = false,
                    Message = createResponse.Item3,
                };
            }
            if (requestEntity.Qr != null)
            {
                return new SignMeResponse
                {
                    IsSuccessful = true,
                    Message = requestEntity.Qr,
                };
            }
            requestEntity.RequestId = (string)createResponse.Item2.GetValue("id");
            var newStatement = _context.Documents.Add(new Document
            {
                RequestId = requestEntity.Id,
                DocumentType = DocumentType.Statement,
                Created = DateTime.UtcNow,

            });
            newStatement.Entity.FileUrl = await _fileService.AddFile(Convert.FromBase64String((string)createResponse.Item2.GetValue("pdf")), request.Id, newStatement.Entity.Id, "pdf");
            requestEntity.Qr = (string)createResponse.Item2.GetValue("qr");
            _context.SaveChanges();
            return new SignMeResponse
            {
                IsSuccessful = true,
                Message = (string)createResponse.Item2.GetValue("qr")
            };
        }
    }
}
