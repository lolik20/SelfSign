using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.Common.ResponseModels
{
    public class AddressResponse
    {
        public string Value { get; set; }
        public long? ShortKladr { get; set; }
        public long? Kladr { get; set; }
        public bool IsDelivery { get; set; } 
    }
}
