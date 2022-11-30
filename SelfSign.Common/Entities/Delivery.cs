using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.Common.Entities
{
    public class Delivery
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string Time { get; set; }
        public string Cladr { get; set; }
        public string Address { get; set; }
        [ForeignKey("Request")]
        public Guid RequestId { get; set; }
        public Request Request { get; set; }
        public VerificationCenter VerificationCenter { get; set; }
    }
}
