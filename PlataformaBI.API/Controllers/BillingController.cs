using DataAccess;
using Domain.Data;
using Domain.Domain;
using FranquiasBusinessIntelligence.API.Services.Session;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PlataformaBI.API.Utils;
using System.Collections.Concurrent;

namespace PlataformaBI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class BillingController : FranquiasBIController
    {
        private readonly FranquiasBIDbContext _context;
        //private readonly ConcurrentDictionary<string, Session> sessions;

        public BillingController(FranquiasBIDbContext context, ConcurrentDictionary<string, Session> sessions)
            : base(sessions)
        {
            _context = context;
            //this.sessions = sessions;
        }

        /// <summary>
        /// Retorna os dados do faturamento conforme o CNPJ da empresa
        /// </summary>
        [HttpGet]
        public IActionResult Get([FromQuery] PageParams pageParams, [FromQuery] FaturamentoParam faturamentoParam)
        {
            if (!UserAuthenticated)
                return Unauthorized();

            this.Session.UpdateLastRequest();

            var faturamento = BuscarFaturamento(pageParams, faturamentoParam);

            if (faturamento == null)
            {
                return NoContent();
            }

            Response.AddPagination(faturamento.CurrentPage, faturamento.ItemsPerPage, faturamento.CountItens, faturamento.TotalPages);


            return Ok(faturamento);
        }

        [NonAction]
        public PageList<IBilling> BuscarFaturamento(PageParams pageParams, FaturamentoParam faturamentoParam)
        {
            IEnumerable<IBilling> faturamento;
            IEnumerable<IBilling> a;

            this.Session.UpdateLastRequest();

            if (faturamentoParam != null)
            {
                //DATA FINAL = DateTime.Today.AddDays(-1);
                //DATA FINAL = DateTime.Today.AddDays(-1);

                if (faturamentoParam.DataInicial == null & faturamentoParam.DataFinal != null)
                {
                    faturamentoParam.DataInicial = faturamentoParam.DataFinal;
                }
                if (faturamentoParam.DataInicial != null & faturamentoParam.DataFinal == null)
                {
                    faturamentoParam.DataFinal = faturamentoParam.DataInicial;
                }

                faturamento = _context
                    .Billing
                    .Where(p =>
                        true
                    );

                if (faturamentoParam.OrdemCrescente == false)
                {
                    a = faturamento.OrderByDescending(p => true).ToArray();
                }
                else
                {
                    a = faturamento.OrderBy(p => true).ToArray();
                }
            }
            else
            {
                a = faturamento = _context
                        .Billing
                        .Where(p => true)
                        .ToArray();
            }

            return PageList<IBilling>.CreateAsyncWithPagination(a, pageParams.CurrentPage, pageParams.ItemsPerPage);
        }
    }
}