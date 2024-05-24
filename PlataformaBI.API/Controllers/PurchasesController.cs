using DataAccess;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PlataformaBI.API.Utils;
using System.Collections.Concurrent;
using Domain.Data;
using Domain.Domain;
using System.Collections.Generic;
using FranquiasBusinessIntelligence.API.Services.Query.Purchases;
using FranquiasBusinessIntelligence.API.Services.Session;

namespace PlataformaBI.API.Controllers
{
    ///
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class PurchasesController : FranquiasBIController
    {
        private readonly SupplierService supplierService;

        private readonly ProductService productService;

        public PurchasesController(ConcurrentDictionary<string, Session> sessions, SupplierService supplierService, ProductService productService)
            : base(sessions)
        {
            this.supplierService = supplierService;
            this.productService = productService;
        }

        /// <summary>
        /// Retorna os dados de compras dos fornecedores conforme os filtros
        /// </summary>
        [HttpGet("purchases")]
        public IActionResult GetPurchases([FromQuery] PageParams pageParams, [FromQuery] IPurchasesParams purchasesParams)
        {
            if (!UserAuthenticated)
                return Unauthorized();

            this.Session.UpdateLastRequest();

            var result = supplierService.FilterPurchases(purchasesParams, pageParams);

            if (result.result == null & result.message == null)
            {
                return NoContent();
            }
            if (result.result == null)
            {
                return BadRequest(result.message);
            }

            var compras = result.result;

            Response.AddPagination(compras.CurrentPage, compras.ItemsPerPage, compras.CountItens, compras.TotalPages);

            return Ok(compras);
        }

        /// <summary>
        /// Retorna os fornecedores
        /// </summary>
        [HttpGet("suppliersList")]
        public IActionResult GetSupplier([FromQuery] PageParams pageParams)
        {
            if (!UserAuthenticated)
                return Unauthorized();

            this.Session.UpdateLastRequest();

            var result = supplierService.GetSupplierList(pageParams);

            if (result.result == null & result.message == null)
            {
                return NoContent();
            }
            if (result.result == null)
            {
                return BadRequest(result.message);
            }

            var suppliers = result.result;

            Response.AddPagination(suppliers.CurrentPage, suppliers.ItemsPerPage, suppliers.CountItens, suppliers.TotalPages);

            return Ok(suppliers);
        }

        /// <summary>
        /// Retorna os produtos
        /// </summary>
        [HttpGet("productsList")]
        public IActionResult GetProduct([FromQuery] PageParams pageParams)
        {
            if (!UserAuthenticated)
                return Unauthorized();

            this.Session.UpdateLastRequest();

            var result = supplierService.GetProductList(pageParams);

            if (result.result == null & result.message == null)
            {
                return NoContent();
            }
            if (result.result == null)
            {
                return BadRequest(result.message);
            }

            var suppliers = result.result;

            Response.AddPagination(suppliers.CurrentPage, suppliers.ItemsPerPage, suppliers.CountItens, suppliers.TotalPages);

            return Ok(suppliers);
        }

        /// <summary>
        /// Retorna todas as informações de fornecedores com maiores valores
        /// </summary>
        [HttpPost("suppliersChart")]
        public IActionResult GetAllSupplierChartByDateAndTotals([FromBody] IPurchasesSupplierParams pageParams)
        {
            if (!UserAuthenticated)
                return Unauthorized();

            this.Session.UpdateLastRequest();

            if (pageParams.Date is null)
                return BadRequest("Missing date parameter!");

            var resultTotalBySupplier = supplierService.GetSupplierChartByDateTotal(pageParams);
            var resultTotalBySupplierPie = supplierService.GetSupplierPieByDateTotal(pageParams);
            var resultChartByDate = supplierService.GetSupplierChartByDate(pageParams);

            var totalBySupplier = resultTotalBySupplier.result;
            var pieSupplierTotal = resultTotalBySupplierPie.result;
            var chartByDate = resultChartByDate.result;

            var combinedResult = new
            {
                TotalByDate = totalBySupplier,
                PieSupplierTotal = pieSupplierTotal,
                Column = chartByDate
            };

            if (combinedResult.TotalByDate!.Equals(null)
                || combinedResult.PieSupplierTotal!.Equals(null)
                || combinedResult.Column!.Equals(null))
                return BadRequest();

            Response.AddPaginationWithout(chartByDate!.CountItens);

            return Ok(combinedResult);
        }

        /// <summary>
        /// Retorna totais e quantidade daquele determinado produto ou empresa
        /// </summary>
        [HttpPost("productsChart")]
        public IActionResult GetProductByTotalValueAndQuantity([FromBody] IPurchasesProductParams pageParams)
        {
            if (!UserAuthenticated)
                return Unauthorized();

            this.Session.UpdateLastRequest();

            if (pageParams.Date is null)
                return BadRequest("Missing date parameter!");

            var resultTotalByProduct = productService.GetProductChartByTotalValueAndQuantity(pageParams);

            var resultChartByProduct = productService.GetProductChartByDate(pageParams);

            var totalByProduct = resultTotalByProduct.result;

            var chartByProduct = resultChartByProduct.result;

            var combinedResult = new
            {
                TotalByProduct = totalByProduct,
                ChartByProduct = chartByProduct,
            };

            if (combinedResult.TotalByProduct!.Equals(null))
                return BadRequest();

            Response.AddPaginationWithout(totalByProduct!.CountItens);

            return Ok(combinedResult);
        }

        /// <summary>
        /// Retorna um mix de produtos daquele determinado fornecedor ou empresa
        /// </summary>
        [HttpPost("productsMix")]
        public IActionResult GetMixProductByDate([FromBody] IPurchasesProductMixParams pageParams)
        {
            if (!UserAuthenticated)
                return Unauthorized();

            this.Session.UpdateLastRequest();

            if (pageParams.Date is null)
                return BadRequest("Missing date parameter!");

            var resultTotalsProduct = productService.GetTotalsByMixProductByDate(pageParams);

            var resultMixProduct = productService.GetMixProductByDate(pageParams);

            var totalsMixProduct = resultTotalsProduct.result;

            var productMix = resultMixProduct.result;

            var combinedResult = new
            {
                TotalsMixProduct = totalsMixProduct,
                MixProduct = productMix,
            };

            Response.AddPaginationWithout(totalsMixProduct!.CountItens);

            return Ok(combinedResult);
        }

        /// <summary>
        /// Retorna a variação de um determinado produto, cnpj ou empresa
        /// </summary>

        [HttpPost("productsVariation")]
        public IActionResult GetVariationProductByDate([FromBody] IPurchasesProductVariationParams pageParams)
        {
            if (!UserAuthenticated)
                return Unauthorized();

            this.Session.UpdateLastRequest();

            if (pageParams.Date is null)
                return BadRequest("Missing date parameter!");

            if (pageParams.ProductCode!.Count() == 0)
                return BadRequest("Missing ProductCode parameter!");

            var resultVariationByProduct = productService.GetVariationProductByDate(pageParams);

            var variationByProduct = resultVariationByProduct.result;

            Response.AddPaginationWithout(variationByProduct!.CountItens);

            return Ok(variationByProduct);
        }
    }
}