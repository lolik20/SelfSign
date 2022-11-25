using System.ComponentModel.DataAnnotations.Schema;

namespace SelfSign.Common.Entities
{
    public class Request
    {
        public Guid Id { get; set; }
        public VerificationCenter VerificationCenter { get; set; }
        public DateTime Created { get; set; }
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public string RequestId { get; set; }
        public User User { get; set; }
        public List<Document> Documents { get; set; }
    }
}
