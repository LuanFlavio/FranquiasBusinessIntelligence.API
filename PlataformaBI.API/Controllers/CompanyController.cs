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
    ///
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class CompanyController : FranquiasBIController
    {
        private readonly FranquiasBIDbContext _context;

        ///
        public CompanyController(FranquiasBIDbContext context, ConcurrentDictionary<string, Session> sessions)
            : base(sessions)
        {
            _context = context;
        }

        /// <summary>
        /// Retorna todos dados de todas empresas
        /// </summary>
        [HttpGet]
        public IActionResult Get([FromQuery] PageParams pageParams, [FromQuery] ICompanyParams companyParams)
        {
            if (!UserAuthenticated)
                return Unauthorized();

            this.Session.UpdateLastRequest();

            IEnumerable<ICompany> query;

            if (companyParams != null)
            {
                query = _context.Company
                .Where(p =>
                    (companyParams.ID != null ? p.Codigo == companyParams.ID : true) &&
                    (pageParams.Description != null ? (
                        p.RazaoSocial!.Contains(pageParams.Description) ||
                        p.NomeFantasia!.Contains(pageParams.Description)
                    ) : true) &&
                    (companyParams.CNPJ != null ? p.CNPJ == companyParams.CNPJ : true) &&
                    (companyParams.Active != null ? p.Ativo == companyParams.Active : true)
                ).ToArray();
            }
            else
            {
                query = _context.Company.ToArray();
            }

            if (query == null)
            {
                return NoContent();
            }

            var empresas = PageList<ICompany>.CreateAsyncWithPagination(query, pageParams.CurrentPage, pageParams.ItemsPerPage);

            Response.AddPagination(empresas.CurrentPage, empresas.ItemsPerPage, empresas.CountItens, empresas.TotalPages);

            return Ok(empresas);
        }

        /// <summary>
        /// Retorna os dados da empresa conforme o CNPJ do mesmo
        /// </summary>
        [HttpGet("{CNPJ}")]
        public IActionResult Get(string CNPJ)
        {
            if (!UserAuthenticated)
                return Unauthorized();

            this.Session.UpdateLastRequest();

            ICompany? empresa = _context.Company.FirstOrDefault(p => p.CNPJ.Equals(Format.GetCNPJ(CNPJ)));

            if (empresa == null)
            {
                return NoContent();
            }

            return Ok(empresa);
        }

        /*        /// <summary>
                /// Retorna os dados da empresa do usuário autenticado
                /// </summary>
                [HttpGet("Conectada")]
                public IActionResult GetEmpresaConectada()
                {
                    if (!UserAuthenticated)
                        return Unauthorized();

                    Empresas empresa = _context.empresas.FirstOrDefault(p => p.Codigo.Equals(Session.usuarioLogado.Empresa)) ?? new Empresas();

                    if (empresa == null || empresa.CNPJ == null)
                    {
                        return NoContent();
                    }

                    return Ok(empresa);
                }*/
    }
}