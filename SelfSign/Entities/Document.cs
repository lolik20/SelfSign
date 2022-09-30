using System.ComponentModel.DataAnnotations.Schema;

namespace SelfSign.Entities
{
    public class Document
    {
        public Guid Id { get; set; }
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User User { get; set; }
        public DocumentType DocumentType { get; set; }
        public string FileUrl { get; set; }
    }
}
