using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfSign.Common.Entities
{
    public class History
    {
        public Guid Id { get; set; }
        [Encrypted]
        public string Event { get; set; }
        public DateTime Created { get; set; }
        [ForeignKey("Request")]
        public Guid RequestId { get; set; }
        public Request Request { get; set; }
    }
}
