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
    public class SnilsUploadRequest:IRequest<SnilsUploadResponse>
    {
        public Guid Id { get; set; }
        public IFormFile file { get; set; }
    }
}
