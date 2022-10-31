using MediatR;
using SelfSign.Common.Entities;
using SelfSign.Common.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.Common.RequestModels
{
    public class IsRequestedRequest:IRequest<IsRequestedResponse>
    {
        public Guid Id { get; set; }
        public VerificationCenter VerificationCenter { get; set; }
    }
}
