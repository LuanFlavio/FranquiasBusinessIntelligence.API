using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Data
{
    [Table("Empresa")]
    public class ICompany
    {
        [Key]
        [JsonPropertyName("id")]
        public int Codigo { get; set; }

        [JsonPropertyName("tradeName")]
        public string NomeFantasia { get; set; } = string.Empty;

        [JsonPropertyName("legalName")]
        public string? RazaoSocial { get; set; }

        [JsonPropertyName("cnpj")]
        public string CNPJ { get; set; } = string.Empty;

        [JsonPropertyName("stateRegistration")]
        public string? InscricaoEstadual { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("responsible")]
        public string? Responsavel { get; set; }

        [JsonPropertyName("address")]
        public string? Endereco { get; set; }

        [JsonPropertyName("neighborhood")]
        public string? Bairro { get; set; }

        [JsonPropertyName("city")]
        public string? Cidade { get; set; }

        [JsonPropertyName("state")]
        public string? UF { get; set; }

        [JsonPropertyName("zipCode")]
        public string? CEP { get; set; }

        [JsonPropertyName("phone")]
        public string? Telefone { get; set; }

        [JsonPropertyName("headquarters")]
        public int? Matriz { get; set; }

        [JsonPropertyName("active")]
        public bool? Ativo { get; set; }
    }

    public class ICompanyParams
    {
        public int? ID { get; set; }
        public string? TradeName { get; set; }
        public string? CNPJ { get; set; }
        public bool? Active { get; set; }

    }
}