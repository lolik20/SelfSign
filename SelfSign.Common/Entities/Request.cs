using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SelfSign.Common.Entities
{
    public class Request
    {
        public Guid Id { get; set; }
        public VerificationCenter VerificationCenter { get; set; }
        public DateTime Created { get; set; }
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        [Encrypted]
        public string RequestId { get; set; }
        public User User { get; set; }
        public bool IsAuthenticated { get; set; }
        public string? Qr { get; set; }
        public List<Document> Documents { get; set; }
        public List<Delivery> Deliveries { get; set; }
        public List<History> History { get; set; }

    }
}
