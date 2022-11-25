using System.ComponentModel.DataAnnotations.Schema;

namespace SelfSign.Common.Entities
{
    public class Document
    {
        public Guid Id { get; set; }
        [ForeignKey("Request")]
        public Guid RequestId { get; set; }
        public Request Request{ get; set; }
        public DocumentType DocumentType { get; set; }
        public string FileUrl { get; set; }
        public DateTime Created { get; set; }
    }
}
