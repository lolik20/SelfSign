using MediatR;
using SelfSign.Common.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.Common.RequestModels
{
    public class IssuedByRequest:IRequest<List<IssuedByResponse>>
    {
        public string query { get; set; }
    }
}
