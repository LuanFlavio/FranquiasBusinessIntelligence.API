namespace Domain.Domain
{
    public class Supplier
    {
        public string CNPJ { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public IEnumerable<Datas> Datas { get; set; }
    }

    public class SupplierPie
    {
        public string CNPJ { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double? TotalValue { get; set; }
        public double? TotalQtd { get; set; }
    }

    public class Totals
    {
        public double? TotalValue { get; set; } = 0;

        public double? TotalQtd { get; set; } = 0;
    }

    public class TotalsMix
    {
        public double? TotalValue { get; set; } = 0;

        public int? TotalCount { get; set; } = 0;
    }
}
