using DataAccess;
using Domain.Data;
using Domain.Domain;
using FranquiasBusinessIntelligence.API.Utils;
using System.Globalization;

namespace FranquiasBusinessIntelligence.API.Services.Query.Purchases
{
    public class ProductService
    {
        private readonly FranquiasBIDbContext _context;

        private ParseIntToMonth parse;

        DateTime lessThanFilter = DateTime.Today.AddDays(-DateTime.Today.Day + 1).AddTicks(-1);

        public ProductService(FranquiasBIDbContext context, ParseIntToMonth parse)
        {
            _context = context;
            this.parse = parse;
        }

        public Result<PageList<Totals>> GetProductChartByTotalValueAndQuantity(IPurchasesProductParams parameters)
        {
            DateTime startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-parameters.Date!.Value);

            var purchases = _context.Purchases
                .Where(p => p.DataEmissao >= startDate && p.DataEmissao <= lessThanFilter)
                .ToList();

            if (parameters.ProductCode!.Count() == 0 && parameters.CompanyId!.Any())
                purchases = purchases
                   .Where(p => parameters.CompanyId!.Count() != 0 ? parameters.CompanyId!.ToList().Contains(p.IdEmpresa!) : true)
                   .OrderByDescending(o => o.Total)
                   .Distinct()
                   .ToList();

            if (parameters.ProductCode!.Count().Equals(0) && parameters.CompanyId!.Count().Equals(0))
            {
                var allPurchases = purchases
                    .OrderByDescending(o => o.Total)
                    .Distinct()
                    .ToList();

                purchases = allPurchases;
            }
            else
            {
                var allPurchases = purchases
                    .Where(p => (parameters.ProductCode!.Count() != 0 ? parameters.ProductCode!.ToList().Contains(p.CodProduto!) : true)
                        && (parameters.CompanyId!.Count() != 0 ? parameters.CompanyId!.ToList().Contains(p.IdEmpresa!) : true))
                    .OrderByDescending(o => o.Total)
                    .Distinct()
                    .ToList();

                purchases = allPurchases;
            }

            var query = purchases
                .GroupBy(p => new { p.CodProduto, p.CGC_Fornecedor })
                .Select(g => new
                {
                    CNPJ = g.Key.CGC_Fornecedor!,
                    TotalValue = Math.Round((double)g.Sum(p => p.Total)!, 2),
                    TotalQtd = g.Sum(p => p.QtdeCompra)
                })
                .GroupBy(s => s.CNPJ)
                .Select(grp => new
                {
                    TotalValue = Math.Round((double)grp.Sum(s => s.TotalValue)!, 2),
                    TotalQtd = Math.Round((double)grp.Sum(s => s.TotalQtd)!, 0)
                })
                .OrderByDescending(o => o.TotalValue)
                .ToList();

            var totalsProduct = new List<Totals>()
            {
                new Totals()
                {
                    TotalValue = Math.Round(query.Sum(item => item.TotalValue), 2),
                    TotalQtd = Math.Round(query.Sum(item => item.TotalQtd), 0)
                }
            };

            var resultPage = PageList<Totals>.CreateAsyncWithoutPagination(totalsProduct);

            var result = new Result<PageList<Totals>>();

            result.result = resultPage;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Result<PageList<ProductChart>> GetProductChartByDate(IPurchasesProductParams parameters)
        {
            bool totalsProducts = false;

            DateTime startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-parameters.Date!.Value);

            var purchases = _context.Purchases
                .Where(p => p.DataEmissao >= startDate && p.DataEmissao <= lessThanFilter)
                .Where(p => (parameters.CompanyId!.Count() != 0 ? parameters.CompanyId!.ToList().Contains(p.IdEmpresa!) : true)
                    && (parameters.ProductCode!.Count() != 0 ? parameters.ProductCode!.ToList().Contains(p.CodProduto!) : true))
                .ToList();

            if (parameters.ProductCode!.Count() == 0 && parameters.CompanyId!.Any())
            {
                var allPurchases = purchases
                 .Where(p => parameters.CompanyId!.Count() != 0 ? parameters.CompanyId!.ToList().Contains(p.IdEmpresa!) : true)
                 .OrderByDescending(o => o.Total)
                 .Distinct()
                 .ToList();

                totalsProducts = true;
            }

            if (parameters.ProductCode!.Count().Equals(0) && parameters.CompanyId!.Count().Equals(0))
            {
                var allPurchases = purchases
                    .OrderByDescending(o => o.Total)
                    .Distinct()
                    .ToList();

                purchases = allPurchases;

                totalsProducts = true;
            }

            var query = new List<ProductChart>();

            var allMonths = Enumerable.Range(0, parameters.Date.Value)
                .Select(i => startDate.AddMonths(i))
                .Select(date => new { date.Month, date.Year })
                .ToList();

            var groupedByMonthAndYear = purchases
                .GroupBy(p => new { p.DataEmissao!.Value.Month, p.DataEmissao!.Value.Year })
                .ToList();

            if (!totalsProducts)
            {
                query = allMonths
                    .Select(monthYear =>
                    {
                        var month = monthYear.Month;
                        var year = monthYear.Year;
                        var productData = groupedByMonthAndYear.FirstOrDefault(g => g.Key.Month == month && g.Key.Year == year);
                        return productData != null
                            ? new ProductChart
                            {
                                TotalValue = Math.Round((double)productData.Sum(p => p.Total)!, 2),
                                TotalQtd = Math.Round((double)productData.Sum(p => p.QtdeCompra)!),
                                Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                                Year = year,
                                Product = productData
                                    .Select(p => new Product
                                    {
                                        Description = p.Produto!,
                                        TotalValue = Math.Round((double)p.Total!, 2),
                                        TotalQuantity = Math.Round((double)p.QtdeCompra!)
                                    })
                                    .ToList()
                            }
                            : new ProductChart
                            {
                                TotalValue = 0,
                                TotalQtd = 0,
                                Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                                Year = year,
                                Product = new List<Product>()
                            };
                    })
                    .OrderByDescending(o => o.TotalValue)
                    .OrderBy(o => parse.ParseToInt(o.Month!))
                    .OrderBy(o => o.Year)
                    .ToList();
            }
            else
            {
                query = allMonths
                    .Select(monthYear =>
                    {
                        var month = monthYear.Month;
                        var year = monthYear.Year;
                        var productData = groupedByMonthAndYear.FirstOrDefault(g => g.Key.Month == month && g.Key.Year == year);
                        return productData != null
                            ? new ProductChart
                            {
                                TotalValue = Math.Round((double)productData.Sum(p => p.Total)!, 2),
                                TotalQtd = Math.Round((double)productData.Sum(p => p.QtdeCompra)!),
                                Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                                Year = year
                            }
                            : new ProductChart
                            {
                                TotalValue = 0,
                                TotalQtd = 0,
                                Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                                Year = year
                            };
                    })
                    .OrderByDescending(o => o.TotalValue)
                    .OrderBy(o => parse.ParseToInt(o.Month!))
                    .OrderBy(o => o.Year)
                    .ToList();
            }

            var resultPage = PageList<ProductChart>.CreateAsyncWithoutPagination(query);

            var result = new Result<PageList<ProductChart>>();

            result.result = resultPage;

            return result;
        }

        public Result<PageList<TotalsMix>> GetTotalsByMixProductByDate(IPurchasesProductMixParams parameters)
        {
            DateTime startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-parameters.Date!.Value);

            var purchases = _context.Purchases
                .Where(p => parameters.SupplierCNPJ!.Count() != 0 ? parameters.SupplierCNPJ!.ToList().Contains(p.CGC_Fornecedor!) : true)
                .Where(p => p.DataEmissao >= startDate && p.DataEmissao <= lessThanFilter)
                .ToList();

            if (parameters.SupplierCNPJ!.Count().Equals(0) && parameters.CompanyId!.Count().Equals(0))
            {
                var allPurchases = purchases
                    .OrderByDescending(o => o.Total)
                    .Distinct()
                    .ToList();

                purchases = allPurchases;
            }

            if (parameters.SupplierCNPJ!.Count().Equals(0) && parameters.CompanyId!.Any())
            {
                var allPurchases = purchases
                    .Where(p => (parameters.SupplierCNPJ!.Count() != 0 ? parameters.SupplierCNPJ!.ToList().Contains(p.CGC_Fornecedor!) : true)
                        && (parameters.CompanyId!.Count() != 0 ? parameters.CompanyId!.ToList().Contains(p.IdEmpresa!) : true))
                    .OrderByDescending(o => o.Total)
                    .Distinct()
                    .ToList();

                purchases = allPurchases;
            }

            var groupedByProduct = purchases
                .GroupBy(p => new { p.CodProduto })
                .Select(s => new TotalsMix()
                {
                    TotalValue = s.Sum(s => s.Total),
                    TotalCount = s.Select(s => s.CodProduto).Distinct().Count()
                });

            var totalsProduct = new List<TotalsMix>()
            {
                new TotalsMix()
                {
                    TotalValue = Math.Round(groupedByProduct.Sum(item => item.TotalValue ?? 0), 2),
                    TotalCount = groupedByProduct.Sum(s => s.TotalCount)
                }
            };

            var resultPage = PageList<TotalsMix>.CreateAsyncWithoutPagination(totalsProduct);

            var result = new Result<PageList<TotalsMix>>();

            result.result = resultPage;

            return result;
        }

        public Result<PageList<ProductChartMix>> GetMixProductByDate(IPurchasesProductMixParams parameters)
        {
            DateTime startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-parameters.Date!.Value);

            var purchases = _context.Purchases
                .Where(p => (parameters.CompanyId!.Count() != 0 ? parameters.CompanyId!.ToList().Contains(p.IdEmpresa!) : true)
                    && (parameters.SupplierCNPJ!.Count() != 0 ? parameters.SupplierCNPJ!.ToList().Contains(p.CGC_Fornecedor!) : true))
                .Where(p => p.DataEmissao >= startDate && p.DataEmissao <= lessThanFilter)
                .OrderByDescending(o => o.Total)
                .ToList();

            var allMonths = Enumerable.Range(0, parameters.Date.Value)
                .Select(i => startDate.AddMonths(i))
                .Select(date => new { date.Month, date.Year })
                .ToList();

            var groupedByProduct = purchases
                .GroupBy(p => new { p.CodProduto })
                .Select(g => new
                {
                    Product = g.Key.CodProduto,
                    g.FirstOrDefault()!.DataEmissao!.Value.Month,
                    g.FirstOrDefault()!.DataEmissao!.Value.Year,
                    TotalValue = g.Sum(p => p.Total),
                    TotalQtd = g.Sum(p => p.QtdeCompra),
                    ProductCount = g.Select(p => p.CodProduto).Distinct().Count()
                })
                .ToList();

            var groupedByMonthAndYear = groupedByProduct
                .GroupBy(p => new { p.Month, p.Year })
                .ToList();

            var query = allMonths
                .Select(monthYear =>
                {
                    var month = monthYear.Month;
                    var year = monthYear.Year;

                    var productData = groupedByMonthAndYear.FirstOrDefault(g => g.Key.Month == month && g.Key.Year == year);
                    return productData != null
                        ? new ProductChartMix
                        {
                            TotalValue = Math.Round((double)productData.Sum(p => p.TotalValue)!, 2),
                            TotalQtd = Math.Round((double)productData.Sum(p => p.TotalQtd)!),
                            ProductCount = productData.Sum(p => p.ProductCount),
                            Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                            Year = year
                        }
                        : new ProductChartMix
                        {
                            TotalValue = 0,
                            TotalQtd = 0,
                            ProductCount = 0,
                            Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                            Year = year
                        };
                })
                .OrderByDescending(o => o.TotalValue)
                .OrderBy(o => parse.ParseToInt(o.Month!))
                .OrderBy(o => o.Year)
                .ToList();

            var result = new Result<PageList<ProductChartMix>>();

            var resultPage = PageList<ProductChartMix>.CreateAsyncWithoutPagination(query
                .Select(g => new ProductChartMix
                {
                    TotalValue = g.TotalValue,
                    TotalQtd = g.TotalQtd,
                    Month = g.Month,
                    Year = g.Year,
                    ProductCount = g.ProductCount
                })
            );

            result.result = resultPage;

            return result;
        }


        public Result<PageList<ProductChartVariation>> GetVariationProductByDate(IPurchasesProductVariationParams parameters)
        {
            DateTime startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-parameters.Date!.Value);

            var purchases = _context.Purchases
                .Where(p => (parameters.ProductCode!.Count() != 0 ? parameters.ProductCode!.ToList().Contains(p.CodProduto!) : true)
                    && (parameters.CompanyId!.Count() != 0 ? parameters.CompanyId!.ToList().Contains(p.IdEmpresa!) : true)
                    && (parameters.SupplierCNPJ!.Count() != 0 ? parameters.SupplierCNPJ!.ToList().Contains(p.CGC_Fornecedor!) : true))
                .Where(p => p.DataEmissao >= startDate && p.DataEmissao <= lessThanFilter)
                .OrderByDescending(o => o.Total)
                .Distinct()
                .ToList();

            var allMonths = Enumerable.Range(0, parameters.Date.Value)
                .Select(i => startDate.AddMonths(i))
                .Select(date => new { date.Month, date.Year })
                .ToList();

            var groupedByMonthAndYear = purchases
                .GroupBy(p => new { p.DataEmissao!.Value.Month, p.DataEmissao!.Value.Year })
                .ToList();

            var boxCostPrevious = 0.0;

            var query = allMonths
                .Select(monthYear =>
                {
                    var month = monthYear.Month;
                    var year = monthYear.Year;

                    var productData = groupedByMonthAndYear.FirstOrDefault(g => g.Key.Month == month && g.Key.Year == year);

                    var currentBoxCost = productData != null ? Math.Round(productData.Average(s => s.CustoCaixa ?? 0), 2) : boxCostPrevious;
                    boxCostPrevious = currentBoxCost;

                    return productData != null
                        ? new ProductChartVariation
                        {
                            TotalValue = Math.Round((double)productData.Sum(p => p.Total)!, 2),
                            TotalQtd = Math.Round((double)productData.Sum(p => p.QtdeCompra)!),
                            Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                            Year = year,
                            Product = productData.SelectMany(p => new List<ProductMix>
                            {
                                new ProductMix
                                {
                                    CodProduto = p.CodProduto,
                                    Description = p.Produto!,
                                    SupplierName = p.Fornecedor!,
                                    TotalValue = p.Total!,
                                    BoxCost = p.CustoCaixa,
                                    TotalQuantity = p.QtdeCompra!
                                }
                            })
                            .GroupBy(s => new { s.CodProduto })
                            .Select(grp => new ProductMix
                            {
                                Description = grp.First().Description,
                                SupplierName = grp.First().SupplierName,
                                BoxCost = Math.Round((double)grp.Average(s => s.BoxCost)!, 2),
                                TotalValue = Math.Round((double)grp.Sum(s => s.TotalValue)!, 2),
                                TotalQuantity = grp.Sum(s => s.TotalQuantity)
                            })
                            .OrderByDescending(o => o.TotalValue)
                            .Distinct()
                            .ToList()
                        }
                        : new ProductChartVariation
                        {
                            TotalValue = 0,
                            TotalQtd = 0,
                            Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                            Year = year,
                            Product = new List<ProductMix>()
                            {
                                new ProductMix()
                                {
                                    BoxCost = boxCostPrevious
                                }
                            }
                        };
                })
                .OrderByDescending(o => o.TotalValue)
                .OrderBy(o => parse.ParseToInt(o.Month!))
                .OrderBy(o => o.Year)
                .ToList();

            var resultPage = PageList<ProductChartVariation>.CreateAsyncWithoutPagination(query);

            var result = new Result<PageList<ProductChartVariation>>();

            result.result = resultPage;

            return result;
        }
    }
}