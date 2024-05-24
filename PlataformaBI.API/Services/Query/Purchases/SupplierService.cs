using DataAccess;
using Domain.Data;
using Domain.Domain;
using FranquiasBusinessIntelligence.API.Utils;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Globalization;


namespace FranquiasBusinessIntelligence.API.Services.Query.Purchases
{
    ///
    public class SupplierService
    {
        private readonly FranquiasBIDbContext _context;

        private ParseIntToMonth parse;

        DateTime lessThanFilter = DateTime.Today.AddDays(-DateTime.Today.Day + 1).AddTicks(-1);

        ///
        public SupplierService(FranquiasBIDbContext context, ParseIntToMonth parse)
        {
            _context = context;
            this.parse = parse;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="pageParams"></param>
        /// <returns></returns>
        public Result<PageList<IPurchases>> FilterPurchases(IPurchasesParams purchaseParams, PageParams pageParam)
        {
            Result<PageList<IPurchases>> result = new()
            {
                result = null,
                message = null
            };

            IEnumerable<IPurchases> compras;
            IEnumerable<IPurchases> temp;
            DateTime? filterDate = null;

            if (purchaseParams != null)
            {
                if (purchaseParams.Date == null)
                {
                    result.message = "Missing param Date. Required!";
                    return result;
                }

                if (purchaseParams.Date != null)
                {
                    filterDate = DateTime.Now.AddMonths((int)-purchaseParams.Date);
                }

                compras = _context
                    .Purchases.AsEnumerable()
                    .Where(p =>
                        (purchaseParams.SupplierCNPJ != null ? purchaseParams.SupplierCNPJ.ToList().Contains(p.CGC_Fornecedor!) : true) &&
                        (purchaseParams.ProductCode != null ? purchaseParams.ProductCode.ToList().Contains(p.CodProduto) : true) &&
                        (purchaseParams.CompanyId != null ? purchaseParams.CompanyId.ToList().Contains(p.IdEmpresa) : true) &&
                        purchaseParams.Date != null ? p.DataEmissao >= filterDate : true
                    ).OrderBy(p => p.Id)
                    .ToArray();

                if (false)
                {
                    temp = compras.OrderByDescending(p => true).ToArray();
                }
                else
                {
                    temp = compras.OrderBy(p => true).ToArray();
                }
            }
            else
            {
                temp = compras = _context
                        .Purchases
                        .Where(p => true) //validação da empresa do usuário logado
                        .ToArray();
            }

            result.result = PageList<IPurchases>.CreateAsyncWithPagination(temp, pageParam.CurrentPage, pageParam.ItemsPerPage);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageParams"></param>
        /// <returns></returns>
        public Result<PageList<Supplier>> GetSupplierList(PageParams pageParams)
        {
            Result<PageList<Supplier>> result = new()
            {
                result = null,
                message = null
            };

            // var result = new List<Product>();
            IEnumerable<Supplier> query;

            query = _context.Purchases
                .Where(p => pageParams.Description != null ? p.Fornecedor!.Contains(pageParams.Description) : true)
                .GroupBy(p => p.CGC_Fornecedor)
                .Select(group => new Supplier
                {
                    CNPJ = group.Key!,
                    Description = group.FirstOrDefault()!.Fornecedor!,
                })
                .ToArray();

            result.result = PageList<Supplier>.CreateAsyncWithPagination(query, pageParams.CurrentPage, pageParams.ItemsPerPage);

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageParams"></param>
        /// <returns></returns>
        public Result<PageList<ProductPerDescription>> GetProductList(PageParams pageParams)
        {
            Result<PageList<ProductPerDescription>> result = new()
            {
                result = null,
                message = null
            };

            IEnumerable<ProductPerDescription> query;

            query = _context.Purchases
                .Where(p => pageParams.Description != null ? p.Produto!.Contains(pageParams.Description) : true)
                .GroupBy(p => new { p.Produto, p.CGC_Fornecedor })
                .Select(group => new ProductPerDescription
                {
                    Id = group.FirstOrDefault()!.CodProduto,
                    Description = group.Key!.Produto,
                    BoxCost = group.Average(p => p.CustoCaixa),
                    SupplierCNPJ = group.FirstOrDefault()!.CGC_Fornecedor!,
                    SupplierName = group.FirstOrDefault()!.Fornecedor!,
                })
                .ToArray()
                .OrderBy(p => p.Description);

            result.result = PageList<ProductPerDescription>.CreateAsyncWithPagination(query, pageParams.CurrentPage, pageParams.ItemsPerPage);

            return result;
        }

        /// <summary>
        /// Retorna todos os fornecedores / CHART
        /// </summary>
        public Result<PageList<Supplier>> GetAllSupplierChart(PageGeneralParams pageParams)
        {
            IEnumerable<Supplier> query = _context.Purchases
                .Where(p => pageParams.Description != null ? p.Fornecedor!.Contains(pageParams.Description) : true)
                .GroupBy(p => p.Referencia)
                .Select(group => new Supplier
                {
                });

            var resultPage = PageList<Supplier>.CreateAsyncWithoutPagination(query);

            var result = new Result<PageList<Supplier>>();

            result.result = resultPage;

            return result;
        }

        public Result<PageList<Totals>> GetSupplierChartByDateTotal(IPurchasesSupplierParams parameters)
        {
            DateTime startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-parameters.Date!.Value);

            var purchases = _context.Purchases
                    .Where(p => (parameters.SupplierCNPJ!.Count() != 0 ? parameters.SupplierCNPJ!.ToList().Contains(p.CGC_Fornecedor!) : true)
                        && (parameters.CompanyId!.Count() != 0 ? parameters.CompanyId!.ToList().Contains(p.IdEmpresa!) : true))
                    .Where(p => p.DataEmissao >= startDate && p.DataEmissao <= lessThanFilter)
                    .ToList();

            if (parameters.SupplierCNPJ!.Count() == 0 && parameters.CompanyId!.Count() == 0)
            {
                purchases = _context.Purchases
                    .Where(p => p.DataEmissao >= startDate && p.DataEmissao <= lessThanFilter)
                    .OrderByDescending(o => o.Total)
                    .ToList();
            }

            if (parameters.SupplierCNPJ!.Count() == 0 && parameters.CompanyId!.Any())
            {
                purchases = _context.Purchases
                    .Where(p => (parameters.SupplierCNPJ!.Count() != 0 ? parameters.SupplierCNPJ!.ToList().Contains(p.CGC_Fornecedor!) : true)
                        && (parameters.CompanyId!.Count() != 0 ? parameters.CompanyId!.ToList().Contains(p.IdEmpresa!) : true))
                    .Where(p => p.DataEmissao >= startDate && p.DataEmissao <= lessThanFilter)
                    .OrderByDescending(o => o.Total)
                    .ToList();
            }

            var query = purchases
                .GroupBy(p => new { p.Fornecedor, p.CGC_Fornecedor })
                .Select(g => new
                {
                    CNPJ = g.Key.CGC_Fornecedor!,
                    TotalValue = Math.Round((double)g.Sum(p => p.Total)!, 2),
                    TotalQtd = g.Sum(p => p.QtdeCompra)
                })
                .GroupBy(s => s.CNPJ)
                .Select(grp => new
                {
                    CNPJ = grp.Key,
                    TotalValue = Math.Round((double)grp.Sum(s => s.TotalValue)!, 2),
                    TotalQtd = Math.Round((double)grp.Sum(s => s.TotalQtd)!, 0)
                })
                .OrderByDescending(o => o.TotalValue)
                .ToList();

            var totalsSupplier = new List<Totals>()
            {
                new Totals()
                {
                    TotalValue = Math.Round(query.Sum(item => item.TotalValue), 2),
                    TotalQtd = Math.Round(query.Sum(item => item.TotalQtd), 0)
                }
            };

            var resultPage = PageList<Totals>.CreateAsyncWithoutPagination(totalsSupplier);

            var result = new Result<PageList<Totals>>();

            result.result = resultPage;

            return result;
        }

        public Result<PageList<SupplierPie>> GetSupplierPieByDateTotal(IPurchasesSupplierParams parameters)
        {
            var totalValueOfOthers = 0.0;
            var totalQtdOfOthers = 0;

            bool othersActive = false;

            DateTime startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-parameters.Date!.Value);

            var purchases = _context.Purchases
                .Where(p => (parameters.SupplierCNPJ!.Count() != 0 ? parameters.SupplierCNPJ!.ToList().Contains(p.CGC_Fornecedor!) : true)
                    && (parameters.CompanyId!.Count() != 0 ? parameters.CompanyId!.ToList().Contains(p.IdEmpresa!) : true))
                .Where(p => p.DataEmissao >= startDate && p.DataEmissao <= lessThanFilter)
                .ToList();

            totalValueOfOthers = (double)purchases.Sum(o => o.Total)!;
            totalQtdOfOthers = (int)purchases.Sum(o => o.QtdeCompra)!;

            if (parameters.SupplierCNPJ!.Count() == 0 && parameters.CompanyId!.Count() == 0)
            {
                purchases = _context.Purchases
                    .Where(p => p.DataEmissao >= startDate && p.DataEmissao <= lessThanFilter)
                    .ToList();

                othersActive = true;
            }

            if (parameters.SupplierCNPJ!.Count() == 0 && parameters.CompanyId!.Any())
            {
                purchases = _context.Purchases
                    .Where(p => (parameters.SupplierCNPJ!.Count() != 0 ? parameters.SupplierCNPJ!.ToList().Contains(p.CGC_Fornecedor!) : true)
                        && (parameters.CompanyId!.Count() != 0 ? parameters.CompanyId!.ToList().Contains(p.IdEmpresa!) : true))
                    .Where(p => p.DataEmissao >= startDate && p.DataEmissao <= lessThanFilter)
                    .ToList();

                othersActive = true;
            }

            var query = purchases
                .GroupBy(p => new { p.Fornecedor, p.CGC_Fornecedor })
                .Select(g => new SupplierPie
                {
                    Description = g.Key.Fornecedor!,
                    CNPJ = g.Key.CGC_Fornecedor!,
                    TotalValue = Math.Round((double)g.Sum(p => p.Total)!, 2),
                    TotalQtd = g.Sum(p => p.QtdeCompra)
                })
                .GroupBy(s => s.CNPJ)
                .Select(grp => new SupplierPie
                {
                    Description = grp.First().Description,
                    CNPJ = grp.Key,
                    TotalValue = Math.Round((double)grp.Sum(s => s.TotalValue)!, 2),
                    TotalQtd = Math.Round((double)grp.Sum(s => s.TotalQtd)!, 0)
                })
                .OrderByDescending(o => o.TotalValue)
                .Take(5)
                .ToList();

            if (query.Count() < 5)
                othersActive = false;

            if (othersActive)
            {
                query.Add(new SupplierPie
                {
                    CNPJ = "Outros",
                    Description = "Outros",
                    TotalValue = Math.Round(totalValueOfOthers - (double)query.Sum(s => s.TotalValue)!, 2),
                    TotalQtd = Math.Round(totalQtdOfOthers! - (double)query.Sum(s => s.TotalQtd)!, 0)
                });
            }

            var resultPage = PageList<SupplierPie>.CreateAsyncWithoutPagination(query);

            var result = new Result<PageList<SupplierPie>>();

            result.result = resultPage;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Result<PageList<Supplier>> GetSupplierChartByDate(IPurchasesSupplierParams parameters)
        {
            bool othersActive = false;

            DateTime startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-parameters.Date!.Value);

            var purchasesQuery = _context.Purchases
                .Where(p => (parameters.SupplierCNPJ!.Count() != 0 ? parameters.SupplierCNPJ!.ToList().Contains(p.CGC_Fornecedor!) : true)
                    && (parameters.CompanyId!.Count() != 0 ? parameters.CompanyId!.ToList().Contains(p.IdEmpresa!) : true))
                .Where(p => p.DataEmissao >= startDate && p.DataEmissao <= lessThanFilter)
                .ToList();

            if (parameters.CompanyId!.Any() && parameters.SupplierCNPJ!.Count() == 0)
            {
                othersActive = true;
            }

            if (parameters.SupplierCNPJ!.Count() == 0 && parameters.CompanyId!.Count() == 0)
            {
                var topSuppliers = purchasesQuery
                    .GroupBy(p => new { p.CGC_Fornecedor })
                    .Select(g => new
                    {
                        g.Key.CGC_Fornecedor,
                        TotalQuantity = g.Sum(p => p.QtdeCompra),
                        TotalValue = g.Sum(p => p.Total)
                    })
                    .OrderByDescending(s => s.TotalValue)
                    .Take(5)
                    .ToList();

                var topSuppliersCGCs = topSuppliers.Select(s => s.CGC_Fornecedor).ToList();

                purchasesQuery = purchasesQuery
                    .Where(p => topSuppliersCGCs.Contains(p.CGC_Fornecedor))
                    .ToList();

                othersActive = true;
            }

            var groupedByMonthAndYear = purchasesQuery
                .GroupBy(p => new { p.DataEmissao!.Value.Month, p.DataEmissao!.Value.Year })
                .ToList();

            var allMonths = Enumerable.Range(0, parameters.Date.Value)
                .Select(i => startDate.AddMonths(i))
                .Select(date => new { date.Month, date.Year })
                .ToList();

            var query = groupedByMonthAndYear
                .OrderBy(o => o.Key.Month)
                .SelectMany(g => g.GroupBy(p => new { p.CGC_Fornecedor })
                    .Select(grp => new Supplier
                    {
                        CNPJ = grp.Key.CGC_Fornecedor!,
                        Description = grp.FirstOrDefault()!.Fornecedor!,
                        Datas = allMonths.Select(monthYear =>
                        {
                            var month = monthYear.Month;
                            var year = monthYear.Year;
                            var purchases = grp.Where(p => p.DataEmissao!.Value.Month == month && p.DataEmissao!.Value.Year == year);
                            return new Datas
                            {
                                Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                                Year = year,
                                TotalValue = Math.Round((double)purchases.Sum(p => p.Total ?? 0), 2),
                                TotalQuantity = purchases.Sum(p => p.QtdeCompra ?? 0)
                            };
                        })
                        .OrderByDescending(o => o.TotalValue)
                        .ToList()
                    }))
                .ToList()
                .GroupBy(s => new { s.CNPJ })
                .Select(grp => new Supplier
                {
                    CNPJ = grp.Key.CNPJ,
                    Description = grp.FirstOrDefault()!.Description,
                    Datas = grp.SelectMany(s => s.Datas)
                        .GroupBy(d => new { d.Month, d.Year })
                        .Select(g => new Datas
                        {
                            Month = g.First().Month,
                            Year = g.First().Year,
                            TotalValue = Math.Round((double)g.Sum(d => d.TotalValue)!, 2),
                            TotalQuantity = Math.Round((double)g.Sum(d => d.TotalQuantity)!, 0)
                        })
                        .OrderByDescending(o => o.TotalValue)
                        .OrderBy(o => parse.ParseToInt(o.Month))
                        .OrderBy(o => o.Year)
                        .ToList()
                })
                .Take(5)
                .ToList();

            if (query.Count() < 5)
                othersActive = false;

            if (othersActive)
            {
                var others = new List<Supplier>();
                var othersDatas = new List<Datas>();

                var cgcFornecedores = query.Select(s => s.CNPJ).Distinct().ToList();

                var allPurchases = _context.Purchases
                    .Where(p => p.DataEmissao >= startDate && p.DataEmissao <= lessThanFilter)
                    .Where(p => (parameters.SupplierCNPJ!.Count() != 0 ? parameters.SupplierCNPJ!.ToList().Contains(p.CGC_Fornecedor!) : true)
                        && (parameters.CompanyId!.Count() != 0 ? parameters.CompanyId!.ToList().Contains(p.IdEmpresa!) : true))
                    .ToList();

                var groupedsByMonthAndYear = allPurchases
                    .GroupBy(p => new { p.DataEmissao!.Value.Month, p.DataEmissao!.Value.Year })
                    .ToList();

                foreach (var monthYear in allMonths)
                {
                    var month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthYear.Month).ToLower();
                    var year = monthYear.Year;

                    var group = groupedsByMonthAndYear.FirstOrDefault(g => g.Key.Month == monthYear.Month && g.Key.Year == year);

                    if (group != null)
                    {
                        var othersPurchases = group
                            .Where(p => !cgcFornecedores.Contains(p.CGC_Fornecedor!))
                            .ToList();

                        var totalValueOutros = othersPurchases.Sum(p => p.Total) ?? 0;
                        var totalQuantityOutros = othersPurchases.Sum(p => p.QtdeCompra) ?? 0;

                        if (totalValueOutros > 0 || totalQuantityOutros > 0)
                        {
                            othersDatas.Add(new Datas
                            {
                                Month = month,
                                Year = year,
                                TotalValue = Math.Round((double)totalValueOutros, 2),
                                TotalQuantity = Math.Round(totalQuantityOutros, 0)
                            });
                        }
                    }
                    else
                    {
                        othersDatas.Add(new Datas
                        {
                            Month = month,
                            Year = year,
                            TotalValue = 0,
                            TotalQuantity = 0
                        });
                    }
                }

                if (othersDatas.Any())
                {
                    others.Add(new Supplier
                    {
                        Description = "Outros",
                        CNPJ = "Outros",
                        Datas = othersDatas
                    });
                }

                query.AddRange(others);
            }


            var resultPage = PageList<Supplier>.CreateAsyncWithoutPagination(query);

            var result = new Result<PageList<Supplier>>();

            result.result = resultPage;

            return result;
        }
    }
}