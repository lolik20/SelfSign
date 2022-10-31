using MediatR;
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
    public class IsRequestedQuery : IRequestHandler<IsRequestedRequest, IsRequestedResponse>
    {
        private readonly ApplicationContext _context;
        public IsRequestedQuery(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<IsRequestedResponse> Handle(IsRequestedRequest request, CancellationToken cancellationToken)
        {
            var oldRequest = _context.Requests.FirstOrDefault(x => x.UserId == request.Id && x.VerificationCenter == request.VerificationCenter);
            if (oldRequest == null)
            {
                return new IsRequestedResponse { IsRequested = false };
            }
            return new IsRequestedResponse { IsRequested = true };
        }
    }
}
