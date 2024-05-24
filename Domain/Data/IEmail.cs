using System.ComponentModel.DataAnnotations;

namespace Domain.Domain
{
    public class IEmail
    {
        [Required]
        [Key]
        public int Id { get; set; }
        public string smtp { get; set; }
        [DataType(DataType.EmailAddress)]
        public string email { get; set; }
        public string senha { get; set; }
        public int porta { get; set; }
        public bool SSL { get; set; }
    }

    public class EmailBody
    {
        public string? corpoEmail { get; set; }
        public string? tituloEmail { get; set; }
    }
}
