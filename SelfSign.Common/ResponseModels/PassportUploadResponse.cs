using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.Common.ResponseModels
{
    public class PassportUploadResponse
    {
        public bool IsSuccess { get; set; }
        public Dictionary<string, string>? Fields { get; set; }
    }
}
