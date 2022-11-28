using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.Common.ResponseModels
{
    public class SignMeResponse
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
    }
}
