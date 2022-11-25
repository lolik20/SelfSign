using MediatR;
using Microsoft.AspNetCore.Http;
using SelfSign.Common.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.Common.RequestModels
{
    public class PassportUploadRequest:IRequest<PassportUploadResponse>
    {
        public Guid Id { get; set; }
        public IFormFile file { get; set; }
    }
}
