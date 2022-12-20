using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Encrypted]
        public string Time { get; set; }
        [Encrypted]
        public string Cladr { get; set; }
        [Encrypted]
        public string Address { get; set; }
        [Encrypted]
        public string PhoneNumber { get; set; }
        [ForeignKey("Request")]
        public Guid RequestId { get; set; }
        public Request Request { get; set; }
        public DeliveryStatus Status { get; set; }
        public int TrackNumber { get; set; }
        public VerificationCenter VerificationCenter { get; set; }
    }
}
