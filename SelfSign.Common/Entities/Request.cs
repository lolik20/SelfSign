﻿using System.ComponentModel.DataAnnotations.Schema;

namespace SelfSign.Common.Entities
{
    public class Request
    {
        public Guid Id { get; set; }
        public VerificationCenter VerificationCenter { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime Created { get; set; }
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public string RequestId { get; set; }
        public User User { get; set; }
        public bool IsAuthenticated { get; set; }
        public List<Document> Documents { get; set; }
        public List<Delivery> Deliveries { get; set; }
        public List<History> History { get; set; }

    }
}
