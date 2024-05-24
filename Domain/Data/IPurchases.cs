using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Data
{
    [Table("Compras")]
    public class IPurchases
    {
        [Key]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("productCode")]
        public int CodProduto { get; set; }

        [JsonPropertyName("product")]
        public string? Produto { get; set; }

        [JsonPropertyName("reference")]
        public string? Referencia { get; set; }

        [JsonPropertyName("unit")]
        public string? Unidade { get; set; }

        [JsonPropertyName("purchaseQuantity")]
        public double? QtdeCompra { get; set; }

        [JsonPropertyName("boxCost")]
        public double? CustoCaixa { get; set; }

        [JsonPropertyName("total")]
        public double? Total { get; set; }

        [JsonPropertyName("supplier")]
        public string? Fornecedor { get; set; }

        [JsonPropertyName("companyId")]
        public int IdEmpresa { get; set; }

        [JsonPropertyName("issuanceDate")]
        public DateTime? DataEmissao { get; set; }

        [JsonPropertyName("entryDate")]
        public DateTime? DataEntrada { get; set; }

        [JsonPropertyName("noteCode")]
        public int? IdCodNota { get; set; }

        [JsonPropertyName("itemCode")]
        public int? IdCodItens { get; set; }

        [JsonPropertyName("supplierCNPJ")]
        public string? CGC_Fornecedor { get; set; }

        [JsonPropertyName("barcode")]
        public string? CodigoBarras { get; set; }
    }


    public class IPurchasesParams
    {
        public IEnumerable<string>? SupplierCNPJ { get; set; }
        public IEnumerable<int>? ProductCode { get; set; }
        public IEnumerable<int>? CompanyId { get; set; }
        public int? Date { get; set; }
    }

    public class IPurchasesSupplierParams
    {
        public IEnumerable<string>? SupplierCNPJ { get; set; }
        //public string? SupplierCNPJ { get; set; }
        /*[JsonPropertyName("barcode")]     
        TEM Q SER PELO CodBarras MAS POR ENQUANTO VOU USAR O ID INTERNO JUNTO AO ID DA EMPRESA
        public string? CodigoBarras { get; set; }*/
        public IEnumerable<int>? CompanyId { get; set; }
        public int? Date { get; set; }
    }

    public class IPurchasesProductParams
    {
        public IEnumerable<int>? ProductCode { get; set; }
        public IEnumerable<int>? CompanyId { get; set; }
        public int? Date { get; set; }
    }

    public class IPurchasesProductMixParams
    {
        public IEnumerable<int>? CompanyId { get; set; }
        public IEnumerable<string>? SupplierCNPJ { get; set; }
        public int? Date { get; set; }
    }

    public class IPurchasesProductVariationParams
    {
        public IEnumerable<int>? ProductCode { get; set; }
        public IEnumerable<int>? CompanyId { get; set; }
        public IEnumerable<string>? SupplierCNPJ { get; set; }
        public int? Date { get; set; }
    }
}
