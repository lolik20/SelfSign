﻿using MediatR;
using Microsoft.AspNetCore.Http;
using SelfSign.Common.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.Common.RequestModels
{
    public class UpdateDeliveryRequest:IRequest<UpdateDeliveryResponse>
    {
        public Guid DeliveryId { get; set; }
        public IFormFile StatementScan { get; set; }
        public IFormFile StatementPhoto { get; set; }
        public IFormFile PassportScan { get; set; }
    }
}