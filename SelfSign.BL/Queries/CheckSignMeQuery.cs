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
    public class CheckSignMeQuery : IRequestHandler<CheckSignMeRequest, CheckSignMeResponse>
    {
        private readonly ApplicationContext _context;
        private readonly ISignmeService _signmeService;
        private readonly IFileService _fileService;
        public CheckSignMeQuery(ApplicationContext context, ISignmeService signmeService, IFileService fileService)
        {
            _context = context;
            _signmeService = signmeService;
            _fileService = fileService;
        }

        public async Task<CheckSignMeResponse> Handle(CheckSignMeRequest request, CancellationToken cancellationToken)
        {
            var user = _context.Users.Include(x => x.Requests.OrderByDescending(x => x.Created)).ThenInclude(x => x.Documents).FirstOrDefault(x => x.Id == request.Id);
            if (user == null || user.Requests.Count(x => x.VerificationCenter == Common.Entities.VerificationCenter.SignMe) == 0)
            {
                return new CheckSignMeResponse
                {
                    Phone = false,
                    Email = false,
                    Inn = false
                };
            }
            var requestEntity = user.Requests.First();
            var preCheck = await _signmeService.PreCheck(user);
            if (preCheck.Pdf != null && requestEntity.Documents.Count(x => x.DocumentType == Common.Entities.DocumentType.Statement) == 0)
            {
                var document = await _context.Documents.AddAsync(new Common.Entities.Document
                {
                    Created = DateTime.UtcNow,
                    DocumentType = Common.Entities.DocumentType.Statement,
                    RequestId = requestEntity.Id
                });
                document.Entity.FileUrl = await _fileService.AddFile(Convert.FromBase64String(preCheck.Pdf), user.Id, document.Entity.Id, "pdf");
                await _context.SaveChangesAsync();
            }
            return new CheckSignMeResponse
            {
                Phone = preCheck.Phone,
                Email = preCheck.Email,
                Inn = preCheck.Inn
            };
        }
    }
}
