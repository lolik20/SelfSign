using MediatR;
using SelfSign.Common.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.Common.RequestModels
{
    public class CreateDeliveryRequest:IRequest<CreateDeliveryResponse>
    {
        public Guid UserId { get; set; }
        public string DeliveryDate { get; set; }
        public long Kladr { get; set; }
        public string Address { get; set; }
        public string Time { get; set; }
        public string PhoneNumber { get; set; }
    }
}
