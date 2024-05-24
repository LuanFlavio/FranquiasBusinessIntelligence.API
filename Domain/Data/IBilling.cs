using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Data
{
    [Table("Faturamento")]
    public class IBilling
    {
        [Key]
        [JsonPropertyName("id")]
        public int Codigo { get; set; }

        [JsonPropertyName("companyCode")]
        public int? CodEmpresa { get; set; }

        [JsonPropertyName("value")]
        public double? Valor { get; set; }

        [JsonPropertyName("competence")]
        public DateTime? Competencia { get; set; }

        [JsonPropertyName("salesQuantity")]
        public int? QtdVendas { get; set; }

        [JsonPropertyName("averageMarkup")]
        public double? MarkupMedio { get; set; }
    }

    public class FaturamentoParam
    {
        public int? Codigo { get; set; }
        public int? Unidade { get; set; }
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public int? SemanaDoAno { get; set; }
        public int? MesDoAno { get; set; }
        public int? Ano { get; set; }
        public bool? OrdemCrescente { get; set; }
    }
}
