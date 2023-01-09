using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.Common.ResponseModels
{
    public class CheckSignMeResponse
    {
        public bool Phone { get; set; }
        public bool Email { get; set; }
        public bool Inn { get; set; }
    }
}
