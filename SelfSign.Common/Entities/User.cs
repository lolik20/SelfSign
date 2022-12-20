using System.ComponentModel.DataAnnotations;

namespace SelfSign.Common.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        [Encrypted]
        public string Name { get; set; }
        [Encrypted]
        public string Surname { get; set; }
        [Encrypted]
        public string Patronymic { get; set; }
        public DateTime RegDate { get; set; }
        [Encrypted]
        public string Serial { get; set; }
        [Encrypted]
        public string Number { get; set; }
        [Encrypted]
        public string RegAddress { get; set; }
        [Encrypted]
        public string BirthPlace { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime IssueDate { get; set; }
        [Encrypted]
        public string Citizenship { get; set; }
        [Encrypted]
        public string SubDivisionCode { get; set; }
        [Encrypted]
        public string SubDivisionAddress { get; set; }
        [Encrypted]
        public string Snils { get; set; }
        [Encrypted]
        public string Inn { get; set; }
        [Encrypted]
        public string Email { get; set; }
        [Encrypted]
        public string Phone { get; set; }
        public Gender Gender { get; set; }
        public List<Request> Requests { get; set; }
    }
}
