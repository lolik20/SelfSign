namespace SelfSign.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public DateTime RegDate { get; set; }
        public string Serial { get; set; }
        public string Number { get; set; }
        public string RegAddress { get; set; }
        public string BirthPlace { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime IssueDate { get; set; }
        public string Citizenship { get; set; }
        public string SubDivisionCode { get; set; }
        public string SubDivisionAddress { get; set; }
        public string Snils { get; set; }
        public string Inn { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public List<Document> Documents { get; set; }
        public int RegionCode { get; set; }
        public SignatureType SignatureType { get; set; }
        public Guid? MyDssRequestId { get; set; }
    }
}
