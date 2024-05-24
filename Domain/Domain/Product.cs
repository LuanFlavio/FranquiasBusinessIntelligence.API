namespace Domain.Domain
{
    public class Product
    { 
        public string Description { get; set; } = string.Empty;
        public double? TotalValue { get; set; }
        public double? TotalQuantity { get; set; }
    }

    public class ProductMix
    {
        public int CodProduto { get; set; }
        public string Description { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public double? TotalValue { get; set; }
        public double? TotalQuantity { get; set; }
        public double? BoxCost { get; set; }
    }

    public class ProductPerDescription
    {
        public int? Id { get; set; }
        public string? Description { get; set; } = string.Empty;
        public double? BoxCost { get; set; }
        public string? SupplierCNPJ { get; set; }
        public string? SupplierName { get; set; }
    }

    public class ProductChart
    {
        public double? TotalValue { get; set; } = 0;
        public double? TotalQtd { get; set; } = 0;
        public string? Month { get; set; }
        public int? Year { get; set; }
        public IEnumerable<Product>? Product { get; set; }
    }

    public class ProductChartMix
    {
        public double? TotalValue { get; set; } = 0;
        public double? TotalQtd { get; set; } = 0;
        public string? Month { get; set; }
        public int? Year { get; set; }
        public int? ProductCount { get; set; }
    }

    public class ProductChartVariation
    {
        public double? TotalValue { get; set; } = 0;
        public double? TotalQtd { get; set; } = 0;
        public string? Month { get; set; }
        public int? Year { get; set; }
        public IEnumerable<ProductMix>? Product { get; set; }
    }
}
